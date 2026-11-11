namespace EquipmentBooking.Application.Catalogue;

public sealed record SaveEquipmentRequest(
    string InventoryNumber,
    string Name,
    string? Description,
    long CategoryId,
    long LocationId,
    decimal Value,
    string ConditionCode,
    bool IsSpecialised,
    long[]? FeatureIds);
