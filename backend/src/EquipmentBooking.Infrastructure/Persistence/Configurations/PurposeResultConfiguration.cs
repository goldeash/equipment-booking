using EquipmentBooking.Domain.Approval;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class PurposeResultConfiguration() : ReferenceDataConfiguration<PurposeResult>(
    "purpose_results",
    "Results of the purpose check by the language model.")
{
    protected override IEnumerable<PurposeResult> SeedData() =>
    [
        new(PurposeResults.Pending, "Not completed yet"),
        new(PurposeResults.Match, "The purpose matches the usage guidelines"),
        new(PurposeResults.NoMatch, "The purpose does not match the usage guidelines"),
        new(PurposeResults.Unavailable, "The assistant was not available"),
        new(PurposeResults.NotRun, "Not run because a policy check failed"),
    ];
}
