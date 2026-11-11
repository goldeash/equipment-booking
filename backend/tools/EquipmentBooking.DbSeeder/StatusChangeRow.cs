namespace EquipmentBooking.DbSeeder;

internal sealed record StatusChangeRow(long BookingId, string? From, string To, DateTimeOffset ChangedAt, long? ChangedBy);
