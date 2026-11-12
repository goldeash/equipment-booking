namespace EquipmentBooking.Domain.Approval;

/// <summary>Stored result of one policy check of an assessment.</summary>
public sealed class ApprovalCheck
{
    private ApprovalCheck()
    {
        CheckTypeCode = string.Empty;
    }

    internal ApprovalCheck(PolicyCheckResult result)
    {
        CheckTypeCode = result.CheckTypeCode;
        Passed = result.Passed;
        ObservedValue = result.ObservedValue;
        LimitValue = result.LimitValue;
    }

    public long AssessmentId { get; private set; }

    public string CheckTypeCode { get; private set; }

    public bool Passed { get; private set; }

    public decimal? ObservedValue { get; private set; }

    public decimal? LimitValue { get; private set; }
}
