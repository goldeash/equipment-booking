using EquipmentBooking.Domain.Approval;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class ApprovalCheckConfiguration : IEntityTypeConfiguration<ApprovalCheck>
{
    public void Configure(EntityTypeBuilder<ApprovalCheck> builder)
    {
        builder.ToTable("approval_checks", table =>
        {
            table.HasComment("Result of every policy check of an assessment with the values it compared.");
            table.HasCheckConstraint("ck_approval_checks_values", "(observed_value IS NULL) = (limit_value IS NULL)");
        });
        builder.HasKey(check => new { check.AssessmentId, check.CheckTypeCode });
        builder.Property(check => check.AssessmentId).HasComment("Assessment the check belongs to.");
        builder.Property(check => check.CheckTypeCode).HasMaxLength(ReferenceDataConfiguration<ApprovalCheckType>.CodeLength).HasComment("Policy check.");
        builder.HasOne<ApprovalCheckType>().WithMany().HasForeignKey(check => check.CheckTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(check => check.Passed).HasComment("The check passed.");
        builder.Property(check => check.ObservedValue).HasPrecision(12, 2).HasComment("Value of the request that was compared, for example the item value in EUR or the loan length in days; null for switches and flags.");
        builder.Property(check => check.LimitValue).HasPrecision(12, 2).HasComment("Limit in force at the time of the assessment, for example the low value threshold or the maximum loan days; null for switches and flags.");
        builder.HasIndex(check => check.CheckTypeCode).HasDatabaseName("ix_approval_checks_check_type_code");
    }
}
