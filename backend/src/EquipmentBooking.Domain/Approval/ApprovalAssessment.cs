using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Approval;

/// <summary>
/// Log entry of the approval route for one request: the result of every policy check, the
/// purpose check and the outcome. Every failed check is stored as a routing reason.
/// </summary>
public sealed class ApprovalAssessment : Entity
{
    private readonly List<ApprovalAssessmentReason> _reasons = [];

    private ApprovalAssessment()
    {
        PurposeResult = string.Empty;
        Outcome = string.Empty;
    }

    public ApprovalAssessment(long bookingId, PolicyEvaluation evaluation, DateTimeOffset assessedAt)
    {
        BookingId = bookingId;
        AssessedAt = assessedAt;
        ValueCheckPassed = evaluation.ValueCheckPassed;
        SpecialisedCheckPassed = evaluation.SpecialisedCheckPassed;
        DurationCheckPassed = evaluation.DurationCheckPassed;
        OrganisationAutoApprovalEnabled = evaluation.OrganisationAutoApprovalEnabled;
        CategoryAutoApprovalEnabled = evaluation.CategoryAutoApprovalEnabled;
        foreach (var reason in evaluation.FailedReasons)
        {
            _reasons.Add(new ApprovalAssessmentReason(reason));
        }

        if (_reasons.Count > 0)
        {
            PurposeResult = PurposeResults.NotRun;
            Outcome = AssessmentOutcomes.Routed;
        }
        else
        {
            PurposeResult = PurposeResults.Pending;
            Outcome = AssessmentOutcomes.Pending;
        }
    }

    public long BookingId { get; private set; }

    public DateTimeOffset AssessedAt { get; private set; }

    public bool ValueCheckPassed { get; private set; }

    public bool SpecialisedCheckPassed { get; private set; }

    public bool DurationCheckPassed { get; private set; }

    public bool OrganisationAutoApprovalEnabled { get; private set; }

    public bool CategoryAutoApprovalEnabled { get; private set; }

    public string PurposeResult { get; private set; }

    public string Outcome { get; private set; }

    public string? Summary { get; private set; }

    public string? ModelName { get; private set; }

    public string? PromptVersion { get; private set; }

    public IReadOnlyCollection<ApprovalAssessmentReason> Reasons => _reasons;

    public bool IsPending => Outcome == AssessmentOutcomes.Pending;

    /// <summary>Records the purpose check result; a match approves, anything else routes to the manager.</summary>
    public void CompletePurposeCheck(PurposeAssessment assessment)
    {
        if (!IsPending)
        {
            throw new DomainException(DomainErrorCodes.InvalidTransition, "The assessment is already completed.");
        }

        PurposeResult = assessment.Result;
        Summary = assessment.Summary;
        ModelName = assessment.ModelName;
        PromptVersion = assessment.PromptVersion;
        if (assessment.Result == PurposeResults.Match)
        {
            Outcome = AssessmentOutcomes.Approved;
            return;
        }

        Outcome = AssessmentOutcomes.Routed;
        var reason = assessment.Result == PurposeResults.NoMatch
            ? RoutingReasonCodes.PurposeNotConfirmed
            : RoutingReasonCodes.AssistantUnavailable;
        _reasons.Add(new ApprovalAssessmentReason(reason));
    }
}
