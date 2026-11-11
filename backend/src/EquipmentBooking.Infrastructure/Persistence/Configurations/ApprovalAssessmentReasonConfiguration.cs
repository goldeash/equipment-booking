using EquipmentBooking.Domain.Approval;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class ApprovalAssessmentReasonConfiguration : IEntityTypeConfiguration<ApprovalAssessmentReason>
{
    public void Configure(EntityTypeBuilder<ApprovalAssessmentReason> builder)
    {
        builder.ToTable("approval_assessment_reasons", table => table.HasComment("Many-to-many link between assessments and every reason why the request was routed."));
        builder.HasKey(reason => new { reason.AssessmentId, reason.RoutingReasonCode });
        builder.Property(reason => reason.AssessmentId).HasComment("Assessment.");
        builder.Property(reason => reason.RoutingReasonCode).HasMaxLength(ReferenceDataConfiguration<RoutingReason>.CodeLength).HasComment("Reason for routing.");
        builder.HasOne<RoutingReason>().WithMany().HasForeignKey(reason => reason.RoutingReasonCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(reason => reason.RoutingReasonCode).HasDatabaseName("ix_approval_assessment_reasons_routing_reason_code");
    }
}
