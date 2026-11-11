using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loans", table =>
        {
            table.HasComment("Issue and return of the item of a booking; one loan per booking.");
            table.HasCheckConstraint("ck_loans_returned_after_issue", "returned_at IS NULL OR returned_at >= issued_at");
            table.HasCheckConstraint("ck_loans_return_complete", "(returned_at IS NULL) = (received_by_user_id IS NULL) AND (returned_at IS NULL) = (return_condition_code IS NULL)");
            table.HasCheckConstraint("ck_loans_damage_comment", "return_condition_code IS DISTINCT FROM 'damaged' OR return_comment IS NOT NULL");
        });
        builder.HasKey(loan => loan.BookingId);
        builder.Property(loan => loan.BookingId).ValueGeneratedNever().HasComment("Booking of the loan (primary key and foreign key).");
        builder.Property(loan => loan.IssuedAt).HasComment("When the item was handed out.");
        builder.Property(loan => loan.IssuedByUserId).HasComment("Administrator who issued the item.");
        builder.HasOne<User>().WithMany().HasForeignKey(loan => loan.IssuedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(loan => loan.DueAt).HasComment("Due time fixed at issue: end date at the organisation return time.");
        builder.Property(loan => loan.ReturnedAt).HasComment("When the item came back.");
        builder.Property(loan => loan.ReceivedByUserId).HasComment("Administrator who received the item.");
        builder.HasOne<User>().WithMany().HasForeignKey(loan => loan.ReceivedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(loan => loan.ReturnConditionCode).HasMaxLength(ReferenceDataConfiguration<ItemCondition>.CodeLength).HasComment("Condition recorded at return.");
        builder.HasOne<ItemCondition>().WithMany().HasForeignKey(loan => loan.ReturnConditionCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(loan => loan.ReturnComment).HasMaxLength(1000).HasComment("Comment at return; required when the item is damaged.");
        builder.HasIndex(loan => loan.DueAt).HasFilter("returned_at IS NULL").HasDatabaseName("ix_loans_open_due_at");
    }
}
