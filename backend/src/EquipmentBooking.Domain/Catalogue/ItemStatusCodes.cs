namespace EquipmentBooking.Domain.Catalogue;

/// <summary>
/// Effective status of an item shown to users. It is not stored: an item is retired when it has a
/// retirement time and out of service while an out of service maintenance period is active.
/// </summary>
public static class ItemStatusCodes
{
    public const string Available = "available";
    public const string Retired = "retired";
    public const string OutOfService = "out_of_service";
}
