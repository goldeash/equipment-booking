namespace EquipmentBooking.Domain.Common;

/// <summary>Base type for entities identified by a database generated key.</summary>
public abstract class Entity
{
    public long Id { get; protected set; }
}
