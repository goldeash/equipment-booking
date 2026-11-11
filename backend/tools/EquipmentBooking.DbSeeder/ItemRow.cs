namespace EquipmentBooking.DbSeeder;

internal sealed class ItemRow
{
    public required long Id { get; init; }

    public required string InventoryNumber { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required CategoryRow Category { get; init; }

    public required long LocationId { get; init; }

    public required decimal Value { get; init; }

    public required string InitialCondition { get; init; }

    public required string Condition { get; set; }

    public required IReadOnlyList<long> FeatureIds { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}
