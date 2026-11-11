using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments", table => table.HasComment("Organisational units; a manager decides on requests of the own department."));
        builder.Property(department => department.Id).HasComment("Surrogate key.");
        builder.Property(department => department.Name).HasMaxLength(100).IsRequired().HasComment("Unique department name.");
        builder.HasIndex(department => department.Name).IsUnique().HasDatabaseName("ux_departments_name");
    }
}
