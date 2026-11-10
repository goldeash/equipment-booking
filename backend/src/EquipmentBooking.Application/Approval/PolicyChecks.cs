using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Common;
using EquipmentBooking.Domain.Settings;

namespace EquipmentBooking.Application.Approval;

/// <summary>Facts about one request that the fixed policy checks evaluate.</summary>
public sealed record ApprovalContext(EquipmentItem Item, Category Category, OrganisationSettings Settings, DateRange Period);

public enum PolicyCheckKind
{
    OrganisationAutoApproval,
    CategoryAutoApproval,
    LowValue,
    NotSpecialised,
    WithinMaxDuration,
}

/// <summary>One fixed policy check of the approval route; new checks are added as new implementations.</summary>
public interface IApprovalPolicyCheck
{
    PolicyCheckKind Kind { get; }

    bool IsSatisfied(ApprovalContext context);
}

internal sealed class OrganisationAutoApprovalCheck : IApprovalPolicyCheck
{
    public PolicyCheckKind Kind => PolicyCheckKind.OrganisationAutoApproval;

    public bool IsSatisfied(ApprovalContext context) => context.Settings.AutoApprovalEnabled;
}

internal sealed class CategoryAutoApprovalCheck : IApprovalPolicyCheck
{
    public PolicyCheckKind Kind => PolicyCheckKind.CategoryAutoApproval;

    public bool IsSatisfied(ApprovalContext context) => context.Category.AutoApprovalEnabled;
}

internal sealed class LowValueCheck : IApprovalPolicyCheck
{
    public PolicyCheckKind Kind => PolicyCheckKind.LowValue;

    public bool IsSatisfied(ApprovalContext context) => context.Settings.IsLowValue(context.Item.Value);
}

internal sealed class NotSpecialisedCheck : IApprovalPolicyCheck
{
    public PolicyCheckKind Kind => PolicyCheckKind.NotSpecialised;

    public bool IsSatisfied(ApprovalContext context) => !context.Item.IsSpecialised && !context.Category.IsSpecialised;
}

internal sealed class WithinMaxDurationCheck : IApprovalPolicyCheck
{
    public PolicyCheckKind Kind => PolicyCheckKind.WithinMaxDuration;

    public bool IsSatisfied(ApprovalContext context) => context.Period.Days <= context.Category.MaxLoanDays;
}

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
