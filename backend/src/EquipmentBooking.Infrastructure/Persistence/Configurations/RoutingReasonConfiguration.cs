using EquipmentBooking.Domain.Approval;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class RoutingReasonConfiguration() : ReferenceDataConfiguration<RoutingReason>(
    "routing_reasons",
    "Reasons why a request is routed to the department manager instead of being approved automatically.")
{
    protected override IEnumerable<RoutingReason> SeedData() =>
    [
        new(RoutingReasonCodes.HighValue, "The item is high value"),
        new(RoutingReasonCodes.Specialised, "The item or its category is specialised"),
        new(RoutingReasonCodes.DurationExceeded, "The loan exceeds the maximum duration of the category"),
        new(RoutingReasonCodes.OrganisationAutoApprovalOff, "Automatic approval is switched off for the organisation"),
        new(RoutingReasonCodes.CategoryAutoApprovalOff, "Automatic approval is switched off for the category"),
        new(RoutingReasonCodes.PurposeNotConfirmed, "The assistant could not confirm the purpose"),
        new(RoutingReasonCodes.AssistantUnavailable, "The AI assistant is not available"),
    ];
}
