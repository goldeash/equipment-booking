using EquipmentBooking.Domain.Approval;

namespace EquipmentBooking.Application.Approval;

internal sealed class LowValueCheck : IApprovalPolicyCheck
{
    public PolicyCheckResult Evaluate(ApprovalContext context) => new(
        ApprovalCheckTypeCodes.ValueThreshold,
        context.Settings.IsLowValue(context.Item.Value),
        context.Item.Value,
        context.Settings.LowValueThreshold);
}
