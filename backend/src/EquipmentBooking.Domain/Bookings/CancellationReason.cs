using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Bookings;

public sealed class CancellationReason : ReferenceData
{
    private CancellationReason()
    {
    }

    public CancellationReason(string code, string name)
        : base(code, name)
    {
    }
}
