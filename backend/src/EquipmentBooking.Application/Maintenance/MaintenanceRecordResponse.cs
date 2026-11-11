namespace EquipmentBooking.Application.Maintenance;

public sealed record MaintenanceRecordResponse(
    long Id,
    string MaintenanceTypeCode,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool TakesOutOfService,
    string Description,
    string RecordedByName,
    DateTimeOffset CreatedAt);
