namespace EquipmentBooking.DbSeeder;

internal sealed record LoanRow(
    long BookingId,
    DateTimeOffset IssuedAt,
    long IssuedBy,
    DateTimeOffset DueAt,
    DateTimeOffset? ReturnedAt,
    long? ReceivedBy,
    string? ReturnCondition,
    string? ReturnComment);
