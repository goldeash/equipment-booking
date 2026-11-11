namespace EquipmentBooking.Application.Catalogue;

public sealed record EquipmentListItem(
    long Id,
    string InventoryNumber,
    string Name,
    long CategoryId,
    string CategoryName,
    long LocationId,
    string LocationName,
    decimal Value,
    string ConditionCode,
    string Status,
    bool IsSpecialised,
    IReadOnlyList<string> Features);
