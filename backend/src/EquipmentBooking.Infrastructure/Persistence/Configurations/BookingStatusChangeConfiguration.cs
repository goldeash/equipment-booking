using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class BookingStatusChangeConfiguration : IEntityTypeConfiguration<BookingStatusChange>
{
    public void Configure(EntityTypeBuilder<BookingStatusChange> builder)
    {
        builder.ToTable("booking_status_history", table => table.HasComment("Every status change of a booking, written by trigger."));
        builder.Property(change => change.Id).HasComment("Surrogate key.");
        builder.Property(change => change.BookingId).HasComment("Changed booking.");
        builder.HasOne<Booking>().WithMany().HasForeignKey(change => change.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(change => change.FromStatusCode).HasMaxLength(ReferenceDataConfiguration<BookingStatus>.CodeLength).HasComment("Previous status; null when the booking was created.");
        builder.HasOne<BookingStatus>().WithMany().HasForeignKey(change => change.FromStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(change => change.ToStatusCode).HasMaxLength(ReferenceDataConfiguration<BookingStatus>.CodeLength).IsRequired().HasComment("New status.");
        builder.HasOne<BookingStatus>().WithMany().HasForeignKey(change => change.ToStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(change => change.ChangedAt).HasDefaultValueSql("now()").HasComment("Change time.");
        builder.Property(change => change.ChangedByUserId).HasComment("User who made the change; null for background jobs and after anonymisation.");
        builder.HasOne<User>().WithMany().HasForeignKey(change => change.ChangedByUserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(change => new { change.BookingId, change.ChangedAt }).HasDatabaseName("ix_booking_status_history_booking_changed_at");
    }
}
