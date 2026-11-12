namespace EquipmentBooking.Domain.Approval;

/// <summary>Results of all fixed policy checks of the approval route for one request, one per check type.</summary>
public sealed class PolicyEvaluation
{
    public PolicyEvaluation(IEnumerable<PolicyCheckResult> checks)
    {
        Checks = [.. checks];
        var codes = Checks.Select(check => check.CheckTypeCode).ToList();
        if (codes.Count != ApprovalCheckTypeCodes.All.Count || codes.Except(ApprovalCheckTypeCodes.All).Any() || codes.Distinct().Count() != codes.Count)
        {
            throw new ArgumentException("An evaluation needs exactly one result for every policy check type.", nameof(checks));
        }
    }

    public IReadOnlyList<PolicyCheckResult> Checks { get; }

    public bool AllPassed => Checks.All(check => check.Passed);
}
