namespace EquipmentBooking.DbSeeder;

internal sealed record NotificationRow(
    long UserId,
    string Type,
    long? BookingId,
    string Title,
    string Body,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);
