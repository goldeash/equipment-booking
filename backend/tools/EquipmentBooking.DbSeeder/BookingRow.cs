namespace EquipmentBooking.DbSeeder;

internal sealed class BookingRow
{
    public required long Id { get; init; }

    public required ItemRow Item { get; init; }

    public required UserRow Requester { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateOnly EndDate { get; init; }

    public required string Purpose { get; init; }

    public required string Status { get; set; }

    public required DateTimeOffset CreatedAt { get; init; }
}
