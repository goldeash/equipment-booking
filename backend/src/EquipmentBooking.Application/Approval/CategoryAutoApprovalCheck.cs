namespace EquipmentBooking.Application.Approval;

internal sealed class CategoryAutoApprovalCheck : IApprovalPolicyCheck
{
    public PolicyCheckKind Kind => PolicyCheckKind.CategoryAutoApproval;

    public bool IsSatisfied(ApprovalContext context) => context.Category.AutoApprovalEnabled;
}
