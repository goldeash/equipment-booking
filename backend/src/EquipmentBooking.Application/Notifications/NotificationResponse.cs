namespace EquipmentBooking.Application.Notifications;

public sealed record NotificationResponse(
    long Id,
    string Type,
    string Title,
    string Body,
    long? BookingId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);
