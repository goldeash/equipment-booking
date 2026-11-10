using System.Collections.Concurrent;
using EquipmentBooking.Application.Bookings;
using EquipmentBooking.Application.Loans;
using EquipmentBooking.Application.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EquipmentBooking.Infrastructure.Jobs;

/// <summary>Names of the scheduled jobs, also used by the internal trigger endpoint.</summary>
public static class JobNames
{
    public const string OverdueLoans = "overdue-loans";
    public const string ReturnReminders = "return-reminders";
    public const string DataRetention = "data-retention";
    public const string ExpiredBookings = "expired-bookings";

    public static readonly IReadOnlyList<string> All = [OverdueLoans, ReturnReminders, DataRetention, ExpiredBookings];
}

/// <summary>Job settings bound from the "Jobs" section.</summary>
public sealed class JobOptions
{
    public const string SectionName = "Jobs";

    /// <summary>Runs the in-process scheduler; disabled in tests.</summary>
    public bool SchedulerEnabled { get; init; } = true;

    /// <summary>Secret for the internal trigger endpoint; the endpoint is disabled when empty.</summary>
    public string? TriggerKey { get; init; }
}

/// <summary>Runs a job in its own scope; the same job never runs twice at the same time in one instance.</summary>
public sealed partial class JobRunner
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobRunner> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);

    public JobRunner(IServiceScopeFactory scopeFactory, ILogger<JobRunner> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>Runs the job and returns the number of processed records, or null when it is already running.</summary>
    public async Task<int?> RunAsync(string name, CancellationToken cancellationToken)
    {
        if (!JobNames.All.Contains(name))
        {
            throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown job.");
        }

        var gate = _locks.GetOrAdd(name, _ => new SemaphoreSlim(1, 1));
        if (!await gate.WaitAsync(0, cancellationToken))
        {
            return null;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var services = scope.ServiceProvider;
            var processed = name switch
            {
                JobNames.OverdueLoans => await services.GetRequiredService<LoanMonitoringService>().MarkOverdueLoansAsync(cancellationToken),
                JobNames.ReturnReminders => await services.GetRequiredService<LoanMonitoringService>().SendReturnRemindersAsync(cancellationToken),
                JobNames.ExpiredBookings => await services.GetRequiredService<BookingExpiryService>().CancelExpiredBookingsAsync(cancellationToken),
                _ => await services.GetRequiredService<RetentionService>().AnonymiseExpiredLoansAsync(cancellationToken),
            };
            LogCompleted(_logger, name, processed);
            return processed;
        }
        finally
        {
            gate.Release();
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Job {JobName} processed {Count} records")]
    private static partial void LogCompleted(ILogger logger, string jobName, int count);
}
