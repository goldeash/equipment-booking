using EquipmentBooking.Domain.Notifications;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class NotificationTypeConfiguration() : ReferenceDataConfiguration<NotificationType>(
    "notification_types",
    "Types of in-app notifications.")
{
    protected override IEnumerable<NotificationType> SeedData() =>
    [
        new(NotificationTypeCodes.RequestApproved, "Request approved"),
        new(NotificationTypeCodes.RequestDeclined, "Request declined"),
        new(NotificationTypeCodes.ReturnReminder, "Return reminder"),
        new(NotificationTypeCodes.LoanOverdue, "Loan overdue"),
        new(NotificationTypeCodes.BookingCancelledByMaintenance, "Booking cancelled because of maintenance"),
        new(NotificationTypeCodes.RequestExpired, "Request expired without a decision"),
        new(NotificationTypeCodes.BookingNotCollected, "Booking cancelled because the item was not collected"),
    ];
}
