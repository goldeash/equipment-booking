using System.Security.Cryptography;
using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Time;
using EquipmentBooking.Domain.Bookings;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Users;

/// <summary>
/// Anonymises personal data of returned loans after the retention period (US-36): the requester is
/// replaced by a random reference, related notifications are deleted and the requester is removed
/// from the status history, while the item history is kept.
/// </summary>
public sealed class RetentionService
{
    private const int BatchSize = 500;

    private readonly IAppDbContext _db;
    private readonly OrganisationCalendar _calendar;

    public RetentionService(IAppDbContext db, OrganisationCalendar calendar)
    {
        _db = db;
        _calendar = calendar;
    }

    public async Task<int> AnonymiseExpiredLoansAsync(CancellationToken cancellationToken)
    {
        var settings = await _calendar.GetSettingsAsync(cancellationToken);
        var now = _calendar.Now;
        var threshold = now.AddDays(-settings.RetentionDays);
        var total = 0;

        while (true)
        {
            var anonymised = await _db.ExecuteInTransactionAsync(async ct =>
            {
                var bookings = await _db.Bookings
                    .Where(booking =>
                        booking.StatusCode == BookingStatusCodes.Returned
                        && booking.RequesterId != null
                        && booking.Loan!.ReturnedAt < threshold)
                    .OrderBy(booking => booking.Id)
                    .Take(BatchSize)
                    .ToListAsync(ct);
                if (bookings.Count == 0)
                {
                    return 0;
                }

                foreach (var booking in bookings)
                {
                    var requesterId = booking.RequesterId!.Value;
                    await _db.Notifications
                        .Where(notification => notification.BookingId == booking.Id && notification.UserId == requesterId)
                        .ExecuteDeleteAsync(ct);
                    await _db.BookingStatusChanges
                        .Where(change => change.BookingId == booking.Id && change.ChangedByUserId == requesterId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(change => change.ChangedByUserId, (long?)null), ct);
                    booking.Anonymise(CreateAnonymousRef(), now);
                }

                await _db.SaveChangesAsync(ct);
                return bookings.Count;
            }, cancellationToken);

            total += anonymised;
            if (anonymised < BatchSize)
            {
                return total;
            }
        }
    }

    private static string CreateAnonymousRef() => $"anon-{Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(8))}";
}
