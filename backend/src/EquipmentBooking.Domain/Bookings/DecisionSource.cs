using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Bookings;

public sealed class DecisionSource : ReferenceData
{
    private DecisionSource()
    {
    }

    public DecisionSource(string code, string name)
        : base(code, name)
    {
    }
}
