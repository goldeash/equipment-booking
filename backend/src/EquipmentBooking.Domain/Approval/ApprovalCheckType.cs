using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Approval;

/// <summary>Fixed policy check of the approval route and the routing reason recorded when it fails.</summary>
public sealed class ApprovalCheckType : ReferenceData
{
    private ApprovalCheckType()
    {
        RoutingReasonCode = string.Empty;
    }

    public ApprovalCheckType(string code, string name, string routingReasonCode)
        : base(code, name)
    {
        RoutingReasonCode = routingReasonCode;
    }

    public string RoutingReasonCode { get; private set; }
}
