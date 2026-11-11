namespace EquipmentBooking.Application.Approval;

internal sealed class WithinMaxDurationCheck : IApprovalPolicyCheck
{
    public PolicyCheckKind Kind => PolicyCheckKind.WithinMaxDuration;

    public bool IsSatisfied(ApprovalContext context) => context.Period.Days <= context.Category.MaxLoanDays;
}
