using EquipmentBooking.Domain.Catalogue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("features", table => table.HasComment("Searchable properties of items, for example wireless or 4K."));
        builder.Property(feature => feature.Id).HasComment("Surrogate key.");
        builder.Property(feature => feature.Name).HasMaxLength(60).IsRequired().HasComment("Unique feature name.");
        builder.HasIndex(feature => feature.Name).IsUnique().HasDatabaseName("ux_features_name");
    }
}
