namespace EquipmentBooking.Domain.Approval;

public sealed class ApprovalAssessmentReason
{
    private ApprovalAssessmentReason()
    {
        RoutingReasonCode = string.Empty;
    }

    internal ApprovalAssessmentReason(string routingReasonCode)
    {
        RoutingReasonCode = routingReasonCode;
    }

    public long AssessmentId { get; private set; }

    public string RoutingReasonCode { get; private set; }
}
