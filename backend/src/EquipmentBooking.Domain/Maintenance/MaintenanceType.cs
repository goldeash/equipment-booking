using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Maintenance;

public sealed class MaintenanceType : ReferenceData
{
    private MaintenanceType()
    {
    }

    public MaintenanceType(string code, string name)
        : base(code, name)
    {
    }
}
