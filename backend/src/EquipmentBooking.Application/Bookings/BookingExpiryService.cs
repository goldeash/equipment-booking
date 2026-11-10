using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Time;
using EquipmentBooking.Application.Notifications;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Notifications;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Bookings;

/// <summary>
/// Daily release of bookings that can no longer be used: requests that no one decided by the end of their
/// start date and approved bookings whose item was not collected by the end of their period. Both are
/// cancelled by the system, so they stop blocking the item.
/// </summary>
public sealed class BookingExpiryService
{
    private readonly IAppDbContext _db;
    private readonly OrganisationCalendar _calendar;
    private readonly NotificationService _notifications;

    public BookingExpiryService(IAppDbContext db, OrganisationCalendar calendar, NotificationService notifications)
    {
        _db = db;
        _calendar = calendar;
        _notifications = notifications;
    }

    /// <summary>Cancels expired bookings, notifies the requesters and, for undecided requests, their department managers.</summary>
    public async Task<int> CancelExpiredBookingsAsync(CancellationToken cancellationToken)
    {
        var today = await _calendar.GetTodayAsync(cancellationToken);
        var expired = await _db.Bookings
            .Include(booking => booking.EquipmentItem)
            .Where(booking =>
                (booking.StatusCode == BookingStatusCodes.Requested && booking.StartDate < today)
                || (booking.StatusCode == BookingStatusCodes.Approved && booking.EndDate < today))
            .ToListAsync(cancellationToken);
        if (expired.Count == 0)
        {
            return 0;
        }

        var requesterIds = expired.Where(booking => booking.RequesterId is not null).Select(booking => booking.RequesterId!.Value).Distinct().ToList();
        var departmentByRequester = await _db.Users
            .Where(user => requesterIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DepartmentId, cancellationToken);
        var managers = await _db.Users
            .Where(user => user.RoleCode == RoleCodes.Manager && user.IsActive)
            .Select(user => new { user.Id, user.DepartmentId })
            .ToListAsync(cancellationToken);

        foreach (var booking in expired)
        {
            var wasRequested = booking.StatusCode == BookingStatusCodes.Requested;
            booking.Expire(today);
            if (booking.RequesterId is not { } requesterId)
            {
                continue;
            }

            var item = booking.EquipmentItem!;
            var period = $"{booking.StartDate:yyyy-MM-dd} to {booking.EndDate:yyyy-MM-dd}";
            if (!wasRequested)
            {
                _notifications.Notify(
                    requesterId,
                    NotificationTypeCodes.BookingNotCollected,
                    booking.Id,
                    "Booking cancelled",
                    $"Your booking of {item.Name} ({item.InventoryNumber}, {period}) was cancelled because the item was not collected.");
                continue;
            }

            _notifications.Notify(
                requesterId,
                NotificationTypeCodes.RequestExpired,
                booking.Id,
                "Request expired",
                $"Your request for {item.Name} ({item.InventoryNumber}, {period}) was cancelled because no decision was made by the start date. Please submit a new request if you still need the item.");
            if (!departmentByRequester.TryGetValue(requesterId, out var departmentId))
            {
                continue;
            }

            foreach (var manager in managers.Where(manager => manager.DepartmentId == departmentId))
            {
                _notifications.Notify(
                    manager.Id,
                    NotificationTypeCodes.RequestExpired,
                    booking.Id,
                    "Request expired",
                    $"The request for {item.Name} ({item.InventoryNumber}, {period}) was cancelled because it was not decided by the start date.");
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return expired.Count;
    }
}
