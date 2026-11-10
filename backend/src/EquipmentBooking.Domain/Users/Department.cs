using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Users;

public sealed class Department : Entity
{
    private Department()
    {
        Name = string.Empty;
    }

    public Department(string name)
    {
        Name = name.Trim();
    }

    public string Name { get; private set; }

    public void Rename(string name) => Name = name.Trim();
}
