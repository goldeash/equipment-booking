using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Time;
using EquipmentBooking.Application.Notifications;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Notifications;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Loans;

/// <summary>Time based checks of loans run by the scheduler: overdue detection and return reminders (US-15, US-18, US-19).</summary>
public sealed class LoanMonitoringService
{
    private readonly IAppDbContext _db;
    private readonly OrganisationCalendar _calendar;
    private readonly NotificationService _notifications;

    public LoanMonitoringService(IAppDbContext db, OrganisationCalendar calendar, NotificationService notifications)
    {
        _db = db;
        _calendar = calendar;
        _notifications = notifications;
    }

    /// <summary>Marks issued loans past their due time as overdue and notifies the employee and the administrators.</summary>
    public async Task<int> MarkOverdueLoansAsync(CancellationToken cancellationToken)
    {
        var now = _calendar.Now;
        var overdue = await _db.Bookings
            .Include(booking => booking.EquipmentItem)
            .Where(booking => booking.StatusCode == BookingStatusCodes.Issued && booking.Loan!.DueAt < now)
            .ToListAsync(cancellationToken);
        if (overdue.Count == 0)
        {
            return 0;
        }

        var administratorIds = await _db.Users
            .Where(user => user.RoleCode == RoleCodes.Administrator && user.IsActive)
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);
        foreach (var booking in overdue)
        {
            booking.MarkOverdue();
            var item = booking.EquipmentItem!;
            if (booking.RequesterId is { } requesterId)
            {
                _notifications.Notify(
                    requesterId,
                    NotificationTypeCodes.LoanOverdue,
                    booking.Id,
                    "Loan overdue",
                    $"{item.Name} ({item.InventoryNumber}) was due on {booking.EndDate:yyyy-MM-dd}. Please return it as soon as possible.");
            }

            foreach (var administratorId in administratorIds)
            {
                _notifications.Notify(
                    administratorId,
                    NotificationTypeCodes.LoanOverdue,
                    booking.Id,
                    "Loan overdue",
                    $"{item.Name} ({item.InventoryNumber}) was not returned by {booking.EndDate:yyyy-MM-dd}.");
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return overdue.Count;
    }

    /// <summary>Sends one reminder for every issued loan that is due tomorrow.</summary>
    public async Task<int> SendReturnRemindersAsync(CancellationToken cancellationToken)
    {
        var tomorrow = (await _calendar.GetTodayAsync(cancellationToken)).AddDays(1);
        var dueTomorrow = await _db.Bookings
            .Where(booking =>
                booking.StatusCode == BookingStatusCodes.Issued
                && booking.EndDate == tomorrow
                && booking.RequesterId != null
                && !_db.Notifications.Any(notification =>
                    notification.BookingId == booking.Id && notification.NotificationTypeCode == NotificationTypeCodes.ReturnReminder))
            .Select(booking => new { booking.Id, RequesterId = booking.RequesterId!.Value, booking.EquipmentItem!.Name, booking.EquipmentItem.InventoryNumber, booking.EndDate })
            .ToListAsync(cancellationToken);

        foreach (var loan in dueTomorrow)
        {
            _notifications.Notify(
                loan.RequesterId,
                NotificationTypeCodes.ReturnReminder,
                loan.Id,
                "Return reminder",
                $"Please return {loan.Name} ({loan.InventoryNumber}) by tomorrow, {loan.EndDate:yyyy-MM-dd}.");
        }

        await _db.SaveChangesAsync(cancellationToken);
        return dueTomorrow.Count;
    }
}
