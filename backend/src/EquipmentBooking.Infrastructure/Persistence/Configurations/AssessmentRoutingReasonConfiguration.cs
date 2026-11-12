using EquipmentBooking.Domain.Approval;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class AssessmentRoutingReasonConfiguration : IEntityTypeConfiguration<AssessmentRoutingReason>
{
    public void Configure(EntityTypeBuilder<AssessmentRoutingReason> builder)
    {
        // A failed check and the purpose result never produce the same reason twice, so the pair is unique.
        builder.HasKey(reason => new { reason.AssessmentId, reason.RoutingReasonCode });
        builder.ToView("v_assessment_routing_reasons");
    }
}
