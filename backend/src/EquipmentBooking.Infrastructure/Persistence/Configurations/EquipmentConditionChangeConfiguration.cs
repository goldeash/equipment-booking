using EquipmentBooking.Domain.Catalogue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class EquipmentConditionChangeConfiguration : IEntityTypeConfiguration<EquipmentConditionChange>
{
    public void Configure(EntityTypeBuilder<EquipmentConditionChange> builder)
    {
        builder.ToTable("equipment_condition_changes", table => table.HasComment("History of condition changes of items, written by trigger."));
        builder.Property(change => change.Id).HasComment("Surrogate key.");
        builder.Property(change => change.EquipmentItemId).HasComment("Item whose condition changed.");
        builder.HasOne<EquipmentItem>().WithMany().HasForeignKey(change => change.EquipmentItemId).OnDelete(DeleteBehavior.Restrict);
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
