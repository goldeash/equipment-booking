using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Common.Time;

/// <summary>Calendar of the organisation: local date and due times in the organisation time zone.</summary>
public sealed class OrganisationCalendar
{
    private readonly IAppDbContext _db;
    private readonly TimeProvider _timeProvider;

    public OrganisationCalendar(IAppDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public DateTimeOffset Now => _timeProvider.GetUtcNow();

    public async Task<OrganisationSettings> GetSettingsAsync(CancellationToken cancellationToken) =>
        await _db.OrganisationSettings.SingleAsync(settings => settings.Id == OrganisationSettings.SingletonId, cancellationToken);

    public async Task<DateOnly> GetTodayAsync(CancellationToken cancellationToken)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        return ToLocalDate(Now, settings.TimeZone);
    }

    public static DateOnly ToLocalDate(DateTimeOffset instant, string timeZoneId)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, zone).DateTime);
    }

    /// <summary>Instant at which a loan ending on <paramref name="endDate"/> becomes due.</summary>
    public static DateTimeOffset GetDueAt(DateOnly endDate, OrganisationSettings settings)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZone);
        var local = endDate.ToDateTime(settings.ReturnTime, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, zone.GetUtcOffset(local)).ToUniversalTime();
    }
}
