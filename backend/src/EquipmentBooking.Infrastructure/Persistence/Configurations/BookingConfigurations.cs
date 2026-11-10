using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Catalogue;
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

internal sealed class BookingDecisionConfiguration : IEntityTypeConfiguration<BookingDecision>
{
    public void Configure(EntityTypeBuilder<BookingDecision> builder)
    {
        builder.ToTable("booking_decisions", table =>
        {
            table.HasComment("Decisions on requests. A date change supersedes the current decision, so a booking can have several.");
            table.HasCheckConstraint("ck_booking_decisions_outcome", "outcome IN ('approved', 'declined')");
            table.HasCheckConstraint("ck_booking_decisions_decline_comment", "outcome <> 'declined' OR comment IS NOT NULL");
            table.HasCheckConstraint(
                "ck_booking_decisions_assistant_never_declines",
                "decision_source_code <> 'ai_assistant' OR (decided_by_user_id IS NULL AND outcome = 'approved')");
            table.HasCheckConstraint("ck_booking_decisions_manager_identified", "decision_source_code <> 'manager' OR decided_by_user_id IS NOT NULL");
        });
        builder.Property(decision => decision.Id).HasComment("Surrogate key.");
        builder.Property(decision => decision.BookingId).HasComment("Decided booking.");
        builder.Property(decision => decision.DecisionSourceCode).HasMaxLength(ReferenceDataConfiguration<DecisionSource>.CodeLength).IsRequired().HasComment("Manager or AI assistant.");
        builder.HasOne<DecisionSource>().WithMany().HasForeignKey(decision => decision.DecisionSourceCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(decision => decision.DecidedByUserId).HasComment("Manager who decided; null for the assistant.");
        builder.HasOne<User>().WithMany().HasForeignKey(decision => decision.DecidedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(decision => decision.Outcome).HasMaxLength(20).IsRequired().HasComment("approved or declined; the assistant can only approve.");
        builder.Property(decision => decision.Comment).HasMaxLength(1000).HasComment("Comment of the manager; required for a decline.");
        builder.Property(decision => decision.DecidedAt).HasComment("Decision time.");
        builder.Property(decision => decision.SupersededAt).HasComment("Set when a date change sends the booking through the route again.");
        builder.Ignore(decision => decision.IsCurrent);
        builder.HasIndex(decision => decision.BookingId)
            .IsUnique()
            .HasFilter("superseded_at IS NULL")
            .HasDatabaseName("ux_booking_decisions_current");
    }
}

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
