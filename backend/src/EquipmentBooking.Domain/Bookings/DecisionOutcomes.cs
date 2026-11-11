namespace EquipmentBooking.Domain.Bookings;

/// <summary>Outcome of a decision on a request. The assistant can only approve.</summary>
public static class DecisionOutcomes
{
    public const string Approved = "approved";
    public const string Declined = "declined";
}
