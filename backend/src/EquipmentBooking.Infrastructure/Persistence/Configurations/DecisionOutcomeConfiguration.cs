using EquipmentBooking.Domain.Bookings;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class DecisionOutcomeConfiguration() : ReferenceDataConfiguration<DecisionOutcome>(
    "decision_outcomes",
    "Outcomes of a decision on a request.")
{
    protected override IEnumerable<DecisionOutcome> SeedData() =>
    [
        new(DecisionOutcomes.Approved, "Approved"),
        new(DecisionOutcomes.Declined, "Declined"),
    ];
}
