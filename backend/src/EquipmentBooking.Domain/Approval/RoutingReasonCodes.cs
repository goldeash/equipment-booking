namespace EquipmentBooking.Domain.Approval;

/// <summary>Reasons why a request is routed to the department manager.</summary>
public static class RoutingReasonCodes
{
    public const string HighValue = "high_value";
    public const string Specialised = "specialised";
    public const string DurationExceeded = "duration_exceeded";
    public const string OrganisationAutoApprovalOff = "org_auto_approval_off";
    public const string CategoryAutoApprovalOff = "category_auto_approval_off";
    public const string PurposeNotConfirmed = "purpose_not_confirmed";
    public const string AssistantUnavailable = "assistant_unavailable";
}
