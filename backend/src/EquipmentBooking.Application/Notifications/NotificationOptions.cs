namespace EquipmentBooking.Application.Notifications;

/// <summary>Notification settings bound from the "Notifications" section.</summary>
public sealed class NotificationOptions
{
    public const string SectionName = "Notifications";

    /// <summary>When true, notifications are also queued for e-mail delivery. No provider is connected yet.</summary>
    public bool EmailEnabled { get; init; }
}
