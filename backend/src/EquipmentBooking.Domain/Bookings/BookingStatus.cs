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
