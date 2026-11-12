using EquipmentBooking.Domain.Approval;

namespace EquipmentBooking.Application.Approval;

internal sealed class OrganisationAutoApprovalCheck : IApprovalPolicyCheck
{
    public PolicyCheckResult Evaluate(ApprovalContext context) => new(
        ApprovalCheckTypeCodes.OrganisationAutoApproval,
        context.Settings.AutoApprovalEnabled);
}
