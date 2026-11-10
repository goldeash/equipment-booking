namespace EquipmentBooking.Domain.Catalogue;

/// <summary>Codes of stored item statuses. Out of service is derived from active maintenance.</summary>
public static class ItemStatusCodes
{
    public const string Available = "available";
    public const string Retired = "retired";

    /// <summary>Effective status shown while an out of service maintenance period is active.</summary>
    public const string OutOfService = "out_of_service";
}
