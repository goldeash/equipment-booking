namespace EquipmentBooking.DbSeeder;

internal sealed record AssessmentRow(
    long Id,
    long BookingId,
    DateTimeOffset AssessedAt,
    IReadOnlyList<CheckRow> Checks,
    string PurposeResult,
    string Outcome);
