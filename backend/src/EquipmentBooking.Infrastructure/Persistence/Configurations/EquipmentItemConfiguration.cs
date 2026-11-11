using EquipmentBooking.Domain.Catalogue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class EquipmentItemConfiguration : IEntityTypeConfiguration<EquipmentItem>
{
    public void Configure(EntityTypeBuilder<EquipmentItem> builder)
    {
        builder.ToTable("equipment_items", table =>
        {
            table.HasComment("Physical items of shared equipment.");
            table.HasCheckConstraint("ck_equipment_items_value", "value >= 0");
            table.HasCheckConstraint("ck_equipment_items_retired", "(status_code = 'retired') = (retired_at IS NOT NULL)");
        });
        builder.Property(item => item.Id).HasComment("Surrogate key.");
        builder.Property(item => item.InventoryNumber).HasMaxLength(30).IsRequired().HasComment("Unique inventory number printed on the item.");
        builder.HasIndex(item => item.InventoryNumber).IsUnique().HasDatabaseName("ux_equipment_items_inventory_number");
        builder.Property(item => item.Name).HasMaxLength(150).IsRequired().HasComment("Item name, for example the model.");
        builder.Property(item => item.Description).HasMaxLength(2000).HasComment("Optional description used by keyword search.");
        builder.Property(item => item.CategoryId).HasComment("Category of the item.");
        builder.HasOne(item => item.Category).WithMany().HasForeignKey(item => item.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(item => item.LocationId).HasComment("Where the item is kept.");
        builder.HasOne(item => item.Location).WithMany().HasForeignKey(item => item.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(item => item.Value).HasPrecision(12, 2).HasComment("Replacement value in EUR; compared with the low value threshold.");
        builder.Property(item => item.ConditionCode).HasMaxLength(ReferenceDataConfiguration<ItemCondition>.CodeLength).IsRequired().HasComment("Current physical condition.");
        builder.HasOne<ItemCondition>().WithMany().HasForeignKey(item => item.ConditionCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(item => item.StatusCode).HasMaxLength(ReferenceDataConfiguration<ItemStatus>.CodeLength).IsRequired().HasComment("Stored status: available or retired.");
        builder.HasOne<ItemStatus>().WithMany().HasForeignKey(item => item.StatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(item => item.IsSpecialised).HasDefaultValue(false).HasComment("Item level override: specialised even if the category is not.");
        builder.Property(item => item.RetiredAt).HasComment("When the item was retired; set together with the status retired.");
        builder.Property(item => item.Version).IsRowVersion();
        builder.Property(item => item.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd().HasComment("Creation time.");
        builder.Property(item => item.UpdatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAddOrUpdate().HasComment("Last change time, set by trigger.");
        builder.Ignore(item => item.IsRetired);

        builder.HasMany(item => item.Features)
            .WithMany()
            .UsingEntity(
                "equipment_item_features",
                right => right.HasOne(typeof(Feature)).WithMany().HasForeignKey("feature_id").OnDelete(DeleteBehavior.Cascade),
                left => left.HasOne(typeof(EquipmentItem)).WithMany().HasForeignKey("equipment_item_id").OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("equipment_item_features", table => table.HasComment("Many-to-many link between items and their features."));
                    join.HasKey("equipment_item_id", "feature_id");
                    join.HasIndex("feature_id").HasDatabaseName("ix_equipment_item_features_feature_id");
                });
        builder.Navigation(item => item.Features).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(item => item.CategoryId).HasDatabaseName("ix_equipment_items_category_id");
        builder.HasIndex(item => item.LocationId).HasDatabaseName("ix_equipment_items_location_id");
    }
}
