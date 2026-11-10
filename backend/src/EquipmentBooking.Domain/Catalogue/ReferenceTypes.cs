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

public sealed class ItemCondition : ReferenceData
{
    private ItemCondition()
    {
    }

    public ItemCondition(string code, string name, int sortOrder)
        : base(code, name)
    {
        SortOrder = sortOrder;
    }

    public int SortOrder { get; private set; }
}
