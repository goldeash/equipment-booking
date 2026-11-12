namespace EquipmentBooking.DbSeeder;

internal sealed record CategorySpec(
    string Name,
    string Description,
    int MaxLoanDays,
    string UsageGuidelines,
    bool IsSpecialised,
    ModelSpec[] Models,
    int ItemCount,
    bool AutoApprovalEnabled = true);
