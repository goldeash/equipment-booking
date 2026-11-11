using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Catalogue;

public sealed class ItemStatus : ReferenceData
{
    private ItemStatus()
    {
    }

    public ItemStatus(string code, string name)
        : base(code, name)
    {
    }
}
