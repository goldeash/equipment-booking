namespace EquipmentBooking.Application.Settings;

public sealed record UpdateApprovalSettingsRequest(decimal LowValueThreshold, bool AutoApprovalEnabled, int RetentionDays, TimeOnly ReturnTime);
