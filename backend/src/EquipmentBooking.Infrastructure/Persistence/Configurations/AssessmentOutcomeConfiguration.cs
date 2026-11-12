using EquipmentBooking.Domain.Approval;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class AssessmentOutcomeConfiguration() : ReferenceDataConfiguration<AssessmentOutcome>(
    "assessment_outcomes",
    "Outcomes of the approval route for a request.")
{
    protected override IEnumerable<AssessmentOutcome> SeedData() =>
    [
        new(AssessmentOutcomes.Pending, "Waiting for the purpose check"),
        new(AssessmentOutcomes.Approved, "Approved automatically"),
        new(AssessmentOutcomes.Routed, "Routed to the department manager"),
    ];
}
