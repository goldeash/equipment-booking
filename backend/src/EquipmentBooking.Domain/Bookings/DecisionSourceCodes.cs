namespace EquipmentBooking.Domain.Bookings;

/// <summary>Who decided on a request.</summary>
public static class DecisionSourceCodes
{
    public const string Manager = "manager";
    public const string AiAssistant = "ai_assistant";
}

/// <summary>Outcome of a decision on a request. The assistant can only approve.</summary>
public static class DecisionOutcomes
{
    public const string Approved = "approved";
    public const string Declined = "declined";
}
