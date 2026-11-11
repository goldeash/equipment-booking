namespace EquipmentBooking.Application.Catalogue;

public sealed record CategoryResponse(
    long Id,
    string Name,
    string? Description,
    int MaxLoanDays,
    string UsageGuidelines,
    bool IsSpecialised,
    bool AutoApprovalEnabled);
