using EquipmentBooking.Domain.Approval;

namespace EquipmentBooking.Application.Approval;

internal sealed class NotSpecialisedCheck : IApprovalPolicyCheck
{
    public PolicyCheckResult Evaluate(ApprovalContext context) => new(
        ApprovalCheckTypeCodes.NotSpecialised,
        !context.Item.IsSpecialised && !context.Category.IsSpecialised);
}
