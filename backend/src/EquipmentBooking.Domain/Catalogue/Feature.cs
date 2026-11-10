using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Catalogue;

/// <summary>Searchable property of items, for example "wireless" or "4K".</summary>
public sealed class Feature : Entity
{
    private Feature()
    {
        Name = string.Empty;
    }

    public Feature(string name)
    {
        Name = name.Trim();
    }

    public string Name { get; private set; }
}
