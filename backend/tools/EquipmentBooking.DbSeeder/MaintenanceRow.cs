namespace EquipmentBooking.DbSeeder;

internal sealed record MaintenanceRow(
    long Id,
    long ItemId,
    string Type,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool TakesOutOfService,
    string Description,
    long RecordedBy,
    DateTimeOffset CreatedAt);
