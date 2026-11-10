using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Users;

public sealed class Role : ReferenceData
{
    private Role()
    {
    }

    public Role(string code, string name)
        : base(code, name)
    {
    }
}
