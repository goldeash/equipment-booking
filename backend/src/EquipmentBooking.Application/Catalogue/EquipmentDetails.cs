namespace EquipmentBooking.Application.Catalogue;

public sealed record EquipmentDetails(
    long Id,
    string InventoryNumber,
    string Name,
    string? Description,
    CategorySummary Category,
    NamedItem Location,
    decimal Value,
    string ConditionCode,
    string Status,
    bool IsSpecialised,
    IReadOnlyList<NamedItem> Features,
    DateTimeOffset? RetiredAt);
