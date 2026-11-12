namespace EquipmentBooking.Application.Approval;

public sealed record AssessmentLogEntry(
    long Id,
    long BookingId,
    string ItemName,
    string InventoryNumber,
    DateTimeOffset AssessedAt,
    IReadOnlyList<AssessmentCheckInfo> Checks,
    string PurposeResult,
    string Outcome,
    IReadOnlyList<string> Reasons);
