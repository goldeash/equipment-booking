using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings", table =>
        {
            table.HasComment("Requests for an item for a date range. A request blocks the item from creation until it is declined, cancelled or returned.");
            table.HasCheckConstraint("ck_bookings_dates", "end_date >= start_date");
            table.HasCheckConstraint("ck_bookings_purpose_length", "char_length(purpose) BETWEEN 3 AND 1000");
            table.HasCheckConstraint("ck_bookings_anonymised", "(requester_id IS NULL) = (anonymised_at IS NOT NULL)");
            table.HasCheckConstraint("ck_bookings_anonymous_ref", "anonymised_at IS NULL OR anonymous_ref IS NOT NULL");
        });
        builder.Property(booking => booking.Id).HasComment("Surrogate key.");
        builder.Property(booking => booking.EquipmentItemId).HasComment("Requested item.");
        builder.HasOne(booking => booking.EquipmentItem).WithMany().HasForeignKey(booking => booking.EquipmentItemId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(booking => booking.RequesterId).HasComment("Employee who requested the item; null after anonymisation.");
        builder.HasOne<User>().WithMany().HasForeignKey(booking => booking.RequesterId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(booking => booking.AnonymousRef).HasMaxLength(40).HasComment("Random reference replacing the requester after the retention period.");
        builder.Property(booking => booking.AnonymisedAt).HasComment("When the personal data of the loan was anonymised.");
        builder.Property(booking => booking.StartDate).HasComment("First day of the booking.");
        builder.Property(booking => booking.EndDate).HasComment("Last day of the booking (inclusive).");
        builder.Property(booking => booking.Purpose).HasMaxLength(1000).IsRequired().HasComment("Purpose stated by the employee; must not contain personal data.");
        builder.Property(booking => booking.StatusCode).HasMaxLength(ReferenceDataConfiguration<BookingStatus>.CodeLength).IsRequired().HasComment("Current status.");
        builder.HasOne<BookingStatus>().WithMany().HasForeignKey(booking => booking.StatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(booking => booking.Version).IsRowVersion();
        builder.Property(booking => booking.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd().HasComment("Creation time.");
        builder.Property(booking => booking.UpdatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAddOrUpdate().HasComment("Last change time, set by trigger.");
        builder.Ignore(booking => booking.Period);
        builder.Ignore(booking => booking.CurrentDecision);
        builder.Ignore(booking => booking.IsBlocking);

        builder.HasMany(booking => booking.Decisions).WithOne().HasForeignKey(decision => decision.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(booking => booking.Decisions).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasOne(booking => booking.Loan).WithOne().HasForeignKey<Loan>(loan => loan.BookingId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(booking => booking.RequesterId).HasDatabaseName("ix_bookings_requester_id");
        builder.HasIndex(booking => new { booking.EquipmentItemId, booking.StartDate }).HasDatabaseName("ix_bookings_item_start_date");
        builder.HasIndex(booking => new { booking.StatusCode, booking.StartDate }).HasDatabaseName("ix_bookings_status_start_date");
    }
}
