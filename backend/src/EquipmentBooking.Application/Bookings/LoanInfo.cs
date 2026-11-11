namespace EquipmentBooking.Application.Bookings;

public sealed record LoanInfo(
    DateTimeOffset IssuedAt,
    DateTimeOffset DueAt,
    DateTimeOffset? ReturnedAt,
    string? ReturnConditionCode,
    string? ReturnComment);
