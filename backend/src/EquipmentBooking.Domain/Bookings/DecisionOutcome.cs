using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Bookings;

public sealed class DecisionOutcome : ReferenceData
{
    private DecisionOutcome()
    {
    }

    public DecisionOutcome(string code, string name)
        : base(code, name)
    {
    }
}
