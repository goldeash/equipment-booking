namespace EquipmentBooking.DbSeeder;

internal sealed record AssessmentRow(
    long Id,
    long BookingId,
    DateTimeOffset AssessedAt,
    bool ValuePassed,
    bool SpecialisedPassed,
    bool DurationPassed,
    string PurposeResult,
    IReadOnlyList<string> Reasons);
