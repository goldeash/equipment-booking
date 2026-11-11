namespace EquipmentBooking.Application.Catalogue;

public sealed record SaveCategoryRequest(
    string Name,
    string? Description,
    int MaxLoanDays,
    string UsageGuidelines,
    bool IsSpecialised,
    bool AutoApprovalEnabled);
