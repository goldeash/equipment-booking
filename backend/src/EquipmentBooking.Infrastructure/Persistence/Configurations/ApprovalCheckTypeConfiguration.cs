using EquipmentBooking.Domain.Approval;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class ApprovalCheckTypeConfiguration() : ReferenceDataConfiguration<ApprovalCheckType>(
    "approval_check_types",
    "Fixed policy checks of the approval route and the routing reason recorded when a check fails.")
{
    public override void Configure(EntityTypeBuilder<ApprovalCheckType> builder)
    {
        base.Configure(builder);
        builder.Property(type => type.RoutingReasonCode).HasMaxLength(CodeLength).IsRequired().HasComment("Routing reason when the check fails.");
        builder.HasOne<RoutingReason>().WithMany().HasForeignKey(type => type.RoutingReasonCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(type => type.RoutingReasonCode).IsUnique().HasDatabaseName("ux_approval_check_types_routing_reason_code");
    }

    protected override IEnumerable<ApprovalCheckType> SeedData() =>
    [
        new(ApprovalCheckTypeCodes.OrganisationAutoApproval, "Automatic approval is switched on for the organisation", RoutingReasonCodes.OrganisationAutoApprovalOff),
        new(ApprovalCheckTypeCodes.CategoryAutoApproval, "Automatic approval is switched on for the category", RoutingReasonCodes.CategoryAutoApprovalOff),
        new(ApprovalCheckTypeCodes.ValueThreshold, "The item value is below the low value threshold", RoutingReasonCodes.HighValue),
        new(ApprovalCheckTypeCodes.NotSpecialised, "Neither the item nor its category is specialised", RoutingReasonCodes.Specialised),
        new(ApprovalCheckTypeCodes.MaxDuration, "The loan does not exceed the maximum duration of the category", RoutingReasonCodes.DurationExceeded),
    ];
}
