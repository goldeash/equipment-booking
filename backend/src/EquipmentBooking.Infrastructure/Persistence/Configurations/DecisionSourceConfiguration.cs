using EquipmentBooking.Domain.Bookings;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class DecisionSourceConfiguration() : ReferenceDataConfiguration<DecisionSource>(
    "decision_sources",
    "Who took a decision on a request.")
{
    protected override IEnumerable<DecisionSource> SeedData() =>
    [
        new(DecisionSourceCodes.Manager, "Department manager"),
        new(DecisionSourceCodes.AiAssistant, "AI assistant"),
    ];
}
