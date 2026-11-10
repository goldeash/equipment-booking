using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Catalogue;

/// <summary>Physical item of shared equipment identified by its inventory number.</summary>
public sealed class EquipmentItem : AuditableEntity
{
    private readonly List<Feature> _features = [];

    private EquipmentItem()
    {
        InventoryNumber = string.Empty;
        Name = string.Empty;
        ConditionCode = string.Empty;
        StatusCode = string.Empty;
    }

    public EquipmentItem(
        string inventoryNumber,
        string name,
        string? description,
        long categoryId,
        long locationId,
        decimal value,
        string conditionCode,
        bool isSpecialised)
    {
        InventoryNumber = inventoryNumber.Trim();
        Name = string.Empty;
        ConditionCode = conditionCode;
        StatusCode = ItemStatusCodes.Available;
        Update(name, description, categoryId, locationId, value, isSpecialised);
    }

    public string InventoryNumber { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public long CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public long LocationId { get; private set; }

    public Location? Location { get; private set; }

    /// <summary>Replacement value used for the low value threshold.</summary>
    public decimal Value { get; private set; }

    public string ConditionCode { get; private set; }

    public string StatusCode { get; private set; }

    /// <summary>Marks a single item as specialised even if its category is not.</summary>
    public bool IsSpecialised { get; private set; }

    public DateTimeOffset? RetiredAt { get; private set; }

    /// <summary>Concurrency token mapped to the PostgreSQL system column xmin.</summary>
    public uint Version { get; private set; }

    public IReadOnlyCollection<Feature> Features => _features;

    public bool IsRetired => StatusCode == ItemStatusCodes.Retired;

    public void Update(string name, string? description, long categoryId, long locationId, decimal value, bool isSpecialised)
    {
        EnsureNotRetired();
        if (value < 0)
        {
            throw new DomainException(DomainErrorCodes.InvalidValue, "The value of an item must not be negative.");
        }

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        CategoryId = categoryId;
        LocationId = locationId;
        Value = value;
        IsSpecialised = isSpecialised;
    }

    public void ChangeInventoryNumber(string inventoryNumber)
    {
        EnsureNotRetired();
        InventoryNumber = inventoryNumber.Trim();
    }

    public void ChangeCondition(string conditionCode) => ConditionCode = conditionCode;

    public void ReplaceFeatures(IEnumerable<Feature> features)
    {
        _features.Clear();
        _features.AddRange(features);
    }

    public void Retire(DateTimeOffset now)
    {
        EnsureNotRetired();
        StatusCode = ItemStatusCodes.Retired;
        RetiredAt = now;
    }

    public void EnsureNotRetired()
    {
        if (IsRetired)
        {
            throw new DomainException(DomainErrorCodes.ItemRetired, "The item is retired and can no longer be changed or booked.");
        }
    }
}
