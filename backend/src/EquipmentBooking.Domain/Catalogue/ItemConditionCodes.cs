namespace EquipmentBooking.Domain.Catalogue;

/// <summary>Codes of the physical condition of an item.</summary>
public static class ItemConditionCodes
{
    public const string New = "new";
    public const string Good = "good";
    public const string Fair = "fair";
    public const string Damaged = "damaged";

    public static readonly IReadOnlyList<string> All = [New, Good, Fair, Damaged];
}
