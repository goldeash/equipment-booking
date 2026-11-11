namespace EquipmentBooking.Application.Maintenance;

public sealed record CreateMaintenanceRequest(
    string MaintenanceTypeCode,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool TakesOutOfService,
    string Description,
    bool ConfirmCancellation);
