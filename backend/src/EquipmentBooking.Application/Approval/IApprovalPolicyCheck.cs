using EquipmentBooking.Domain.Approval;

namespace EquipmentBooking.Application.Approval;

/// <summary>One fixed policy check of the approval route; every check type has exactly one implementation.</summary>
public interface IApprovalPolicyCheck
{
    PolicyCheckResult Evaluate(ApprovalContext context);
}
