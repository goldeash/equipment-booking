using EquipmentBooking.Domain.Catalogue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations", table => table.HasComment("Places where items are kept when not on loan."));
        builder.Property(location => location.Id).HasComment("Surrogate key.");
        builder.Property(location => location.Name).HasMaxLength(100).IsRequired().HasComment("Unique location name, for example a room or a storage.");
        builder.HasIndex(location => location.Name).IsUnique().HasDatabaseName("ux_locations_name");
    }
}
