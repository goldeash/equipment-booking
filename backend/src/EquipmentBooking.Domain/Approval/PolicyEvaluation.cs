namespace EquipmentBooking.Domain.Approval;

/// <summary>Results of all fixed policy checks of the approval route for one request.</summary>
public sealed record PolicyEvaluation(
    bool ValueCheckPassed,
    bool SpecialisedCheckPassed,
    bool DurationCheckPassed,
    bool OrganisationAutoApprovalEnabled,
    bool CategoryAutoApprovalEnabled)
{
    /// <summary>Routing reasons of every failed check; empty when all checks passed.</summary>
    public IReadOnlyList<string> FailedReasons
    {
        get
        {
            var reasons = new List<string>();
            if (!OrganisationAutoApprovalEnabled)
            {
                reasons.Add(RoutingReasonCodes.OrganisationAutoApprovalOff);
            }

            if (!CategoryAutoApprovalEnabled)
            {
                reasons.Add(RoutingReasonCodes.CategoryAutoApprovalOff);
            }

            if (!ValueCheckPassed)
            {
                reasons.Add(RoutingReasonCodes.HighValue);
            }

            if (!SpecialisedCheckPassed)
            {
                reasons.Add(RoutingReasonCodes.Specialised);
            }

            if (!DurationCheckPassed)
            {
                reasons.Add(RoutingReasonCodes.DurationExceeded);
            }

            return reasons;
        }
    }

    public bool AllPassed => FailedReasons.Count == 0;
}

/// <summary>Result of the purpose check returned by a purpose assessor.</summary>
public sealed record PurposeAssessment(string Result, string? Summary, string? ModelName, string? PromptVersion)
{
    public static PurposeAssessment Unavailable { get; } = new(PurposeResults.Unavailable, null, null, null);
}
