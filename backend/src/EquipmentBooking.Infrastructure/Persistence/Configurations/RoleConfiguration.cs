using EquipmentBooking.Domain.Users;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration() : ReferenceDataConfiguration<Role>("roles", "Fixed list of user roles.")
{
    protected override IEnumerable<Role> SeedData() =>
    [
        new(RoleCodes.Employee, "Employee"),
        new(RoleCodes.Manager, "Department manager"),
        new(RoleCodes.Administrator, "Administrator"),
    ];
}
