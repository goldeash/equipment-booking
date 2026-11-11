namespace EquipmentBooking.Application.Approval;

internal sealed class OrganisationAutoApprovalCheck : IApprovalPolicyCheck
{
    public PolicyCheckKind Kind => PolicyCheckKind.OrganisationAutoApproval;

    public bool IsSatisfied(ApprovalContext context) => context.Settings.AutoApprovalEnabled;
}
