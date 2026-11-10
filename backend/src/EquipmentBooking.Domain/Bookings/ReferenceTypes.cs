using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Bookings;

public sealed class BookingStatus : ReferenceData
{
    private BookingStatus()
    {
    }

    public BookingStatus(string code, string name)
        : base(code, name)
    {
    }
}

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
