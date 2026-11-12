using EquipmentBooking.Domain.Approval;

namespace EquipmentBooking.Application.Approval;

internal sealed class CategoryAutoApprovalCheck : IApprovalPolicyCheck
{
    public PolicyCheckResult Evaluate(ApprovalContext context) => new(
        ApprovalCheckTypeCodes.CategoryAutoApproval,
        context.Category.AutoApprovalEnabled);
}
