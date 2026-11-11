namespace EquipmentBooking.Application.Settings;

public sealed record ApprovalSettingsResponse(
    decimal LowValueThreshold,
    bool AutoApprovalEnabled,
    int RetentionDays,
    TimeOnly ReturnTime,
    string TimeZone);
