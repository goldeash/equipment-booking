namespace EquipmentBooking.DbSeeder;

internal sealed record ConditionChangeRow(long ItemId, string? From, string To, DateTimeOffset ChangedAt, long? ChangedBy);
