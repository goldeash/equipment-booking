namespace EquipmentBooking.Domain.Common;

/// <summary>Entity with creation and modification timestamps maintained by the database.</summary>
public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset UpdatedAt { get; protected set; }
}
