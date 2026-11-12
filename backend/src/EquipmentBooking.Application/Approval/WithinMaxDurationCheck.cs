using EquipmentBooking.Domain.Approval;

namespace EquipmentBooking.Application.Approval;

internal sealed class WithinMaxDurationCheck : IApprovalPolicyCheck
{
    public PolicyCheckResult Evaluate(ApprovalContext context) => new(
        ApprovalCheckTypeCodes.MaxDuration,
        context.Period.Days <= context.Category.MaxLoanDays,
        context.Period.Days,
        context.Category.MaxLoanDays);
}
