namespace EquipmentBooking.Application.Approval;

internal sealed class LowValueCheck : IApprovalPolicyCheck
{
    public PolicyCheckKind Kind => PolicyCheckKind.LowValue;

    public bool IsSatisfied(ApprovalContext context) => context.Settings.IsLowValue(context.Item.Value);
}
