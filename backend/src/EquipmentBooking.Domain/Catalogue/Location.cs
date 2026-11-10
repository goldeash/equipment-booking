using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Catalogue;

/// <summary>Place where an item is kept when it is not on loan.</summary>
public sealed class Location : Entity
{
    private Location()
    {
        Name = string.Empty;
    }

    public Location(string name)
    {
        Name = name.Trim();
    }

    public string Name { get; private set; }

    public void Rename(string name) => Name = name.Trim();
}
