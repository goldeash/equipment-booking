namespace EquipmentBooking.Domain.Approval;

/// <summary>Routing reason of an assessment derived by the database from a failed check or the purpose result.</summary>
public sealed class AssessmentRoutingReason
{
    private AssessmentRoutingReason()
    {
        RoutingReasonCode = string.Empty;
    }

    public long AssessmentId { get; private set; }

    public string RoutingReasonCode { get; private set; }
}
