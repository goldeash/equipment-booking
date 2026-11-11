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

    public PolicyEvaluation Evaluate(ApprovalContext context)
    {
        var results = _checks.ToDictionary(check => check.Kind, check => check.IsSatisfied(context));
        bool Passed(PolicyCheckKind kind) => !results.TryGetValue(kind, out var passed) || passed;
        return new PolicyEvaluation(
            ValueCheckPassed: Passed(PolicyCheckKind.LowValue),
            SpecialisedCheckPassed: Passed(PolicyCheckKind.NotSpecialised),
            DurationCheckPassed: Passed(PolicyCheckKind.WithinMaxDuration),
            OrganisationAutoApprovalEnabled: Passed(PolicyCheckKind.OrganisationAutoApproval),
            CategoryAutoApprovalEnabled: Passed(PolicyCheckKind.CategoryAutoApproval));
    }
}
