using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Maintenance;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.ToTable("maintenance_records", table =>
        {
            table.HasComment("Maintenance and repairs of items; an out of service record blocks bookings in its period.");
            table.HasCheckConstraint("ck_maintenance_records_dates", "end_date IS NULL OR end_date >= start_date");
        });
        builder.Property(record => record.Id).HasComment("Surrogate key.");
        builder.Property(record => record.EquipmentItemId).HasComment("Maintained item.");
        builder.HasOne<EquipmentItem>().WithMany().HasForeignKey(record => record.EquipmentItemId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(record => record.MaintenanceTypeCode).HasMaxLength(ReferenceDataConfiguration<MaintenanceType>.CodeLength).IsRequired().HasComment("Type of work.");
        builder.HasOne<MaintenanceType>().WithMany().HasForeignKey(record => record.MaintenanceTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(record => record.StartDate).HasComment("First day of the maintenance.");
        builder.Property(record => record.EndDate).HasComment("Last day of the maintenance; null while open ended.");
        builder.Property(record => record.TakesOutOfService).HasComment("The item cannot be booked during the period.");
        builder.Property(record => record.Description).HasMaxLength(2000).IsRequired().HasComment("What was done or found.");
        builder.Property(record => record.RecordedByUserId).HasComment("Administrator who recorded the entry.");
        builder.HasOne<User>().WithMany().HasForeignKey(record => record.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(record => record.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd().HasComment("Creation time.");
        builder.HasIndex(record => new { record.EquipmentItemId, record.StartDate }).HasDatabaseName("ix_maintenance_records_item_start_date");
    }
}
