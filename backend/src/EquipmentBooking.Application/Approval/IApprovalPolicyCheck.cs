namespace EquipmentBooking.Application.Approval;

/// <summary>One fixed policy check of the approval route; new checks are added as new implementations.</summary>
public interface IApprovalPolicyCheck
{
    PolicyCheckKind Kind { get; }

    bool IsSatisfied(ApprovalContext context);
}
