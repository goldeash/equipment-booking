namespace EquipmentBooking.Application.Approval;

internal sealed class NotSpecialisedCheck : IApprovalPolicyCheck
{
    public PolicyCheckKind Kind => PolicyCheckKind.NotSpecialised;

    public bool IsSatisfied(ApprovalContext context) => !context.Item.IsSpecialised && !context.Category.IsSpecialised;
}
