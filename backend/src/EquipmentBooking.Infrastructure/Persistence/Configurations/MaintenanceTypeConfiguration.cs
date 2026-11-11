using EquipmentBooking.Domain.Maintenance;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class MaintenanceTypeConfiguration() : ReferenceDataConfiguration<MaintenanceType>(
    "maintenance_types",
    "Types of maintenance work.")
{
    protected override IEnumerable<MaintenanceType> SeedData() =>
    [
        new(MaintenanceTypeCodes.Inspection, "Inspection"),
        new(MaintenanceTypeCodes.Repair, "Repair"),
        new(MaintenanceTypeCodes.Calibration, "Calibration"),
        new(MaintenanceTypeCodes.Cleaning, "Cleaning"),
    ];
}
