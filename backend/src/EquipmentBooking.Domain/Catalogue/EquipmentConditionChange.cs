using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Catalogue;

/// <summary>Change of the condition of an item, written by a database trigger.</summary>
public sealed class EquipmentConditionChange : Entity
{
    private EquipmentConditionChange()
    {
        ToConditionCode = string.Empty;
    }

    public long EquipmentItemId { get; private set; }

    public string? FromConditionCode { get; private set; }

    public string ToConditionCode { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }

    public long? ChangedByUserId { get; private set; }
}
