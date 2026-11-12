using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Approval;

/// <summary>
/// Log entry of the approval route for one request: the result of every policy check with the values
/// it compared, the purpose check and the outcome. The routing reasons are derived from the failed
/// checks and the purpose result, so they are never stored twice.
/// </summary>
public sealed class ApprovalAssessment : Entity
{
    private readonly List<ApprovalCheck> _checks = [];

    private ApprovalAssessment()
    {
        PurposeResult = string.Empty;
        Outcome = string.Empty;
    }

    public ApprovalAssessment(long bookingId, PolicyEvaluation evaluation, DateTimeOffset assessedAt)
    {
        BookingId = bookingId;
        AssessedAt = assessedAt;
        _checks.AddRange(evaluation.Checks.Select(check => new ApprovalCheck(check)));
        if (evaluation.AllPassed)
        {
            PurposeResult = PurposeResults.Pending;
            Outcome = AssessmentOutcomes.Pending;
        }
        else
        {
            PurposeResult = PurposeResults.NotRun;
            Outcome = AssessmentOutcomes.Routed;
        }
    }

    public long BookingId { get; private set; }

    public DateTimeOffset AssessedAt { get; private set; }

    public string PurposeResult { get; private set; }

    public string Outcome { get; private set; }

    public string? Summary { get; private set; }

    public string? ModelName { get; private set; }

    public string? PromptVersion { get; private set; }

    public IReadOnlyCollection<ApprovalCheck> Checks => _checks;

    public bool IsPending => Outcome == AssessmentOutcomes.Pending;

    /// <summary>Records the purpose check result; a match approves, anything else routes to the manager.</summary>
    public void CompletePurposeCheck(PurposeAssessment assessment)
    {
        if (!IsPending)
        {
            throw new DomainException(DomainErrorCodes.InvalidTransition, "The assessment is already completed.");
        }

        if (assessment.Result is not (PurposeResults.Match or PurposeResults.NoMatch or PurposeResults.Unavailable))
        {
            throw new ArgumentException($"'{assessment.Result}' is not a result of a completed purpose check.", nameof(assessment));
        }

        PurposeResult = assessment.Result;
        Summary = assessment.Summary;
        ModelName = assessment.ModelName;
        PromptVersion = assessment.PromptVersion;
        Outcome = assessment.Result == PurposeResults.Match ? AssessmentOutcomes.Approved : AssessmentOutcomes.Routed;
    }
}
