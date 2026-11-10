using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Approval;

public sealed class RoutingReason : ReferenceData
{
    private RoutingReason()
    {
    }

    public RoutingReason(string code, string name)
        : base(code, name)
    {
    }
}
