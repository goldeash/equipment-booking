namespace EquipmentBooking.Domain.Notifications;

/// <summary>Delivery states of the e-mail copy of a notification.</summary>
public static class EmailStatuses
{
    public const string Pending = "pending";
    public const string Sent = "sent";
    public const string Failed = "failed";
    public const string NotRequired = "not_required";
}
