namespace EquipmentBooking.Domain.Approval;

/// <summary>Result of the purpose check returned by a purpose assessor.</summary>
public sealed record PurposeAssessment(string Result, string? Summary, string? ModelName, string? PromptVersion)
{
    public static PurposeAssessment Unavailable { get; } = new(PurposeResults.Unavailable, null, null, null);
}
