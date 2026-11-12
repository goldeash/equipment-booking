using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class BookingDecisionConfiguration : IEntityTypeConfiguration<BookingDecision>
{
    public void Configure(EntityTypeBuilder<BookingDecision> builder)
    {
        builder.ToTable("booking_decisions", table =>
        {
            table.HasComment("Decisions on requests. A date change supersedes the current decision, so a booking can have several.");
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
        builder.Property(decision => decision.Outcome).HasMaxLength(ReferenceDataConfiguration<DecisionOutcome>.CodeLength).IsRequired().HasComment("Approved or declined; the assistant can only approve.");
        builder.HasOne<DecisionOutcome>().WithMany().HasForeignKey(decision => decision.Outcome).OnDelete(DeleteBehavior.Restrict);
        builder.Property(decision => decision.Comment).HasMaxLength(1000).HasComment("Comment of the manager; required for a decline.");
        builder.Property(decision => decision.DecidedAt).HasComment("Decision time.");
        builder.Property(decision => decision.SupersededAt).HasComment("Set when a date change sends the booking through the route again.");
        builder.Ignore(decision => decision.IsCurrent);
        builder.HasIndex(decision => decision.Outcome).HasDatabaseName("ix_booking_decisions_outcome");
        builder.HasIndex(decision => decision.DecisionSourceCode).HasDatabaseName("ix_booking_decisions_decision_source_code");
        builder.HasIndex(decision => decision.DecidedByUserId).HasDatabaseName("ix_booking_decisions_decided_by_user_id");
        builder.HasIndex(decision => decision.BookingId)
            .IsUnique()
            .HasFilter("superseded_at IS NULL")
            .HasDatabaseName("ux_booking_decisions_current");
    }
}
