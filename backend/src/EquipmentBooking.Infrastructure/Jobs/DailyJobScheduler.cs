using EquipmentBooking.Application.Common.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EquipmentBooking.Infrastructure.Jobs;

/// <summary>
/// Runs the jobs once a day at fixed local times of the organisation. Overdue detection runs shortly
/// after the common return time, so every overdue loan is flagged within one hour (KPI-2). Expired
/// bookings are released shortly after midnight, when the previous day is over. The database
/// is not polled in between, which keeps the serverless database idle. An external daily trigger can
/// call the same jobs when the host was asleep.
/// </summary>
public sealed partial class DailyJobScheduler : BackgroundService
{
    private static readonly TimeSpan OverdueDelayAfterReturnTime = TimeSpan.FromMinutes(5);
    private static readonly TimeOnly ReminderTime = new(9, 0);
    private static readonly TimeOnly RetentionTime = new(3, 0);
    private static readonly TimeOnly ExpiryTime = new(0, 10);

    private readonly JobRunner _runner;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly JobOptions _options;
    private readonly ILogger<DailyJobScheduler> _logger;

    public DailyJobScheduler(
        JobRunner runner,
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        IOptions<JobOptions> options,
        ILogger<DailyJobScheduler> logger)
    {
        _runner = runner;
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.SchedulerEnabled)
        {
            return;
        }

        await RunSafelyAsync(JobNames.OverdueLoans, stoppingToken);
        await RunSafelyAsync(JobNames.ExpiredBookings, stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var (job, runAt) = await GetNextRunAsync(stoppingToken);
                var delay = runAt - _timeProvider.GetUtcNow();
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, _timeProvider, stoppingToken);
                }

                await RunSafelyAsync(job, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                LogSchedulingFailed(_logger, exception);
                await Task.Delay(TimeSpan.FromMinutes(15), _timeProvider, stoppingToken);
            }
        }
    }

    private async Task<(string Job, DateTimeOffset RunAt)> GetNextRunAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var settings = await scope.ServiceProvider.GetRequiredService<OrganisationCalendar>().GetSettingsAsync(cancellationToken);
        var zone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZone);
        var now = _timeProvider.GetUtcNow();
        var candidates = new[]
        {
            (JobNames.OverdueLoans, NextOccurrence(now, settings.ReturnTime.Add(OverdueDelayAfterReturnTime), zone)),
            (JobNames.ReturnReminders, NextOccurrence(now, ReminderTime, zone)),
            (JobNames.DataRetention, NextOccurrence(now, RetentionTime, zone)),
            (JobNames.ExpiredBookings, NextOccurrence(now, ExpiryTime, zone)),
        };
        return candidates.MinBy(candidate => candidate.Item2);
    }

    /// <summary>Next instant after <paramref name="now"/> at which the local clock shows <paramref name="time"/>.</summary>
    internal static DateTimeOffset NextOccurrence(DateTimeOffset now, TimeOnly time, TimeZoneInfo zone)
    {
        var localNow = TimeZoneInfo.ConvertTime(now, zone);
        var date = DateOnly.FromDateTime(localNow.DateTime);
        for (var offset = 0; offset < 3; offset++)
        {
            var local = date.AddDays(offset).ToDateTime(time, DateTimeKind.Unspecified);
            var instant = new DateTimeOffset(local, zone.GetUtcOffset(local));
            if (instant > now)
            {
                return instant.ToUniversalTime();
            }
        }

        return now.AddDays(1);
    }

    private async Task RunSafelyAsync(string job, CancellationToken cancellationToken)
    {
        try
        {
            await _runner.RunAsync(job, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogJobFailed(_logger, exception, job);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Scheduled job {JobName} failed")]
    private static partial void LogJobFailed(ILogger logger, Exception exception, string jobName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Scheduling of jobs failed; retrying in 15 minutes")]
    private static partial void LogSchedulingFailed(ILogger logger, Exception exception);
}
