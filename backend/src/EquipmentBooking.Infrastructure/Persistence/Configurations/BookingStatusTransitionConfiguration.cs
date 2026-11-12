using EquipmentBooking.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class BookingStatusTransitionConfiguration : IEntityTypeConfiguration<BookingStatusTransition>
{
    public void Configure(EntityTypeBuilder<BookingStatusTransition> builder)
    {
        builder.ToTable("booking_status_transitions", table => table.HasComment("Allowed status changes of a booking, enforced by the trigger fn_check_booking_transition."));
        builder.HasKey(transition => new { transition.FromStatusCode, transition.ToStatusCode });
        builder.Property(transition => transition.FromStatusCode).HasMaxLength(ReferenceDataConfiguration<BookingStatus>.CodeLength).HasComment("Current status.");
        builder.HasOne<BookingStatus>().WithMany().HasForeignKey(transition => transition.FromStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(transition => transition.ToStatusCode).HasMaxLength(ReferenceDataConfiguration<BookingStatus>.CodeLength).HasComment("Status the booking may change to.");
        builder.HasOne<BookingStatus>().WithMany().HasForeignKey(transition => transition.ToStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(transition => transition.ToStatusCode).HasDatabaseName("ix_booking_status_transitions_to_status_code");
        builder.HasData(BookingStatusCodes.Transitions.Select(transition => new BookingStatusTransition(transition.From, transition.To)));
    }
}
