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

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories", table =>
        {
            table.HasComment("Groups of items with the same loan limit, usage guidelines and approval settings.");
            table.HasCheckConstraint("ck_categories_max_loan_days", "max_loan_days > 0");
        });
        builder.Property(category => category.Id).HasComment("Surrogate key.");
        builder.Property(category => category.Name).HasMaxLength(100).IsRequired().HasComment("Unique category name.");
        builder.HasIndex(category => category.Name).IsUnique().HasDatabaseName("ux_categories_name");
        builder.Property(category => category.Description).HasMaxLength(500).HasComment("Optional description.");
        builder.Property(category => category.MaxLoanDays).HasComment("Longest loan in days that can be approved automatically.");
        builder.Property(category => category.UsageGuidelines).HasMaxLength(2000).IsRequired()
            .HasComment("What the items may be used for; the assistant compares the stated purpose with this text.");
        builder.Property(category => category.IsSpecialised).HasDefaultValue(false).HasComment("Requests for items of a specialised category always go to a manager.");
        builder.Property(category => category.AutoApprovalEnabled).HasDefaultValue(true).HasComment("Switch for automatic approval of this category.");
    }
}

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

internal sealed class EquipmentConditionChangeConfiguration : IEntityTypeConfiguration<EquipmentConditionChange>
{
    public void Configure(EntityTypeBuilder<EquipmentConditionChange> builder)
    {
        builder.ToTable("equipment_condition_changes", table => table.HasComment("History of condition changes of items, written by trigger."));
        builder.Property(change => change.Id).HasComment("Surrogate key.");
        builder.Property(change => change.EquipmentItemId).HasComment("Item whose condition changed.");
        builder.HasOne<EquipmentItem>().WithMany().HasForeignKey(change => change.EquipmentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(change => change.FromConditionCode).HasMaxLength(ReferenceDataConfiguration<ItemCondition>.CodeLength).HasComment("Previous condition; null for a new item.");
        builder.HasOne<ItemCondition>().WithMany().HasForeignKey(change => change.FromConditionCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(change => change.ToConditionCode).HasMaxLength(ReferenceDataConfiguration<ItemCondition>.CodeLength).IsRequired().HasComment("New condition.");
        builder.HasOne<ItemCondition>().WithMany().HasForeignKey(change => change.ToConditionCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(change => change.ChangedAt).HasDefaultValueSql("now()").HasComment("Change time.");
        builder.Property(change => change.ChangedByUserId).HasComment("User who caused the change, if known.");
        builder.HasOne<Domain.Users.User>().WithMany().HasForeignKey(change => change.ChangedByUserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(change => new { change.EquipmentItemId, change.ChangedAt }).HasDatabaseName("ix_equipment_condition_changes_item_changed_at");
    }
}
