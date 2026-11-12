using EquipmentBooking.Domain.Approval;

namespace EquipmentBooking.Application.Approval;

/// <summary>Runs every policy check without stopping at the first failure, so all routing reasons are recorded.</summary>
public sealed class ApprovalPolicyEvaluator
{
    private readonly IReadOnlyList<IApprovalPolicyCheck> _checks;

    public ApprovalPolicyEvaluator(IEnumerable<IApprovalPolicyCheck> checks)
    {
        _checks = [.. checks];
    }

    public PolicyEvaluation Evaluate(ApprovalContext context) =>
        new(_checks.Select(check => check.Evaluate(context)));
}
