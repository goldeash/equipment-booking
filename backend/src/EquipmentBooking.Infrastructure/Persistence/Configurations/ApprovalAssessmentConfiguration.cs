using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class ApprovalAssessmentConfiguration : IEntityTypeConfiguration<ApprovalAssessment>
{
    public void Configure(EntityTypeBuilder<ApprovalAssessment> builder)
    {
        builder.ToTable("approval_assessments", table => table.HasComment("Decision log of the approval route: the purpose check and the outcome; the results of the policy checks are in approval_checks."));
        builder.Property(assessment => assessment.Id).HasComment("Surrogate key.");
        builder.Property(assessment => assessment.BookingId).HasComment("Assessed booking.");
        builder.HasOne<Booking>().WithMany().HasForeignKey(assessment => assessment.BookingId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(assessment => assessment.AssessedAt).HasComment("Time of the assessment.");
        builder.Property(assessment => assessment.PurposeResult).HasMaxLength(ReferenceDataConfiguration<PurposeResult>.CodeLength).IsRequired().HasComment("Result of the purpose check by the language model.");
        builder.HasOne<PurposeResult>().WithMany().HasForeignKey(assessment => assessment.PurposeResult).OnDelete(DeleteBehavior.Restrict);
        builder.Property(assessment => assessment.Outcome).HasMaxLength(ReferenceDataConfiguration<AssessmentOutcome>.CodeLength).IsRequired().HasComment("Pending, approved automatically or routed to the manager.");
        builder.HasOne<AssessmentOutcome>().WithMany().HasForeignKey(assessment => assessment.Outcome).OnDelete(DeleteBehavior.Restrict);
        builder.Property(assessment => assessment.Summary).HasMaxLength(1000).HasComment("Short summary for the manager prepared by the assistant.");
        builder.Property(assessment => assessment.ModelName).HasMaxLength(100).HasComment("Language model used for the purpose check.");
        builder.Property(assessment => assessment.PromptVersion).HasMaxLength(20).HasComment("Version of the prompt template used.");
        builder.Ignore(assessment => assessment.IsPending);
        builder.HasMany(assessment => assessment.Checks).WithOne().HasForeignKey(check => check.AssessmentId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(assessment => assessment.Checks).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(assessment => assessment.BookingId).HasDatabaseName("ix_approval_assessments_booking_id");
        builder.HasIndex(assessment => assessment.AssessedAt).HasDatabaseName("ix_approval_assessments_assessed_at");
        builder.HasIndex(assessment => assessment.Outcome).HasDatabaseName("ix_approval_assessments_outcome");
        builder.HasIndex(assessment => assessment.PurposeResult).HasDatabaseName("ix_approval_assessments_purpose_result");
    }
}
