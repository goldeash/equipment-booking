using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class ApprovalAssessmentConfiguration : IEntityTypeConfiguration<ApprovalAssessment>
{
    public void Configure(EntityTypeBuilder<ApprovalAssessment> builder)
    {
        builder.ToTable("approval_assessments", table =>
        {
            table.HasComment("Decision log of the approval route: result of every policy check, the purpose check and the outcome.");
            table.HasCheckConstraint("ck_approval_assessments_purpose_result", "purpose_result IN ('pending', 'match', 'no_match', 'unavailable', 'not_run')");
            table.HasCheckConstraint("ck_approval_assessments_outcome", "outcome IN ('pending', 'approved', 'routed')");
            table.HasCheckConstraint("ck_approval_assessments_approved_needs_match", "outcome <> 'approved' OR purpose_result = 'match'");
        });
        builder.Property(assessment => assessment.Id).HasComment("Surrogate key.");
        builder.Property(assessment => assessment.BookingId).HasComment("Assessed booking.");
        builder.HasOne<Booking>().WithMany().HasForeignKey(assessment => assessment.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(assessment => assessment.AssessedAt).HasComment("Time of the assessment.");
        builder.Property(assessment => assessment.ValueCheckPassed).HasComment("The item value is below the low value threshold.");
        builder.Property(assessment => assessment.SpecialisedCheckPassed).HasComment("Neither the item nor its category is specialised.");
        builder.Property(assessment => assessment.DurationCheckPassed).HasComment("The loan does not exceed the maximum duration of the category.");
        builder.Property(assessment => assessment.OrganisationAutoApprovalEnabled).HasComment("Automatic approval was switched on for the organisation.");
        builder.Property(assessment => assessment.CategoryAutoApprovalEnabled).HasComment("Automatic approval was switched on for the category.");
        builder.Property(assessment => assessment.PurposeResult).HasMaxLength(20).IsRequired().HasComment("Result of the purpose check by the language model.");
        builder.Property(assessment => assessment.Outcome).HasMaxLength(20).IsRequired().HasComment("pending, approved or routed to the manager.");
        builder.Property(assessment => assessment.Summary).HasMaxLength(1000).HasComment("Short summary for the manager prepared by the assistant.");
        builder.Property(assessment => assessment.ModelName).HasMaxLength(100).HasComment("Language model used for the purpose check.");
        builder.Property(assessment => assessment.PromptVersion).HasMaxLength(20).HasComment("Version of the prompt template used.");
        builder.Ignore(assessment => assessment.IsPending);
        builder.HasMany(assessment => assessment.Reasons).WithOne().HasForeignKey(reason => reason.AssessmentId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(assessment => assessment.Reasons).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(assessment => assessment.BookingId).HasDatabaseName("ix_approval_assessments_booking_id");
        builder.HasIndex(assessment => assessment.AssessedAt).HasDatabaseName("ix_approval_assessments_assessed_at");
    }
}
