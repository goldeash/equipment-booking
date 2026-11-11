namespace EquipmentBooking.Application.Approval;

public sealed record AssessmentLogEntry(
    long Id,
    long BookingId,
    string ItemName,
    string InventoryNumber,
    DateTimeOffset AssessedAt,
    bool ValueCheckPassed,
    bool SpecialisedCheckPassed,
    bool DurationCheckPassed,
    bool OrganisationAutoApprovalEnabled,
    bool CategoryAutoApprovalEnabled,
    string PurposeResult,
    string Outcome,
    IReadOnlyList<string> Reasons);
