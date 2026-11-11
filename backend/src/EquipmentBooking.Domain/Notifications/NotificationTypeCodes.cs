namespace EquipmentBooking.Domain.Notifications;

/// <summary>Codes of notification types.</summary>
public static class NotificationTypeCodes
{
    public const string RequestApproved = "request_approved";
    public const string RequestDeclined = "request_declined";
    public const string ReturnReminder = "return_reminder";
    public const string LoanOverdue = "loan_overdue";
    public const string BookingCancelledByMaintenance = "booking_cancelled_by_maintenance";
    public const string RequestExpired = "request_expired";
    public const string BookingNotCollected = "booking_not_collected";
}
