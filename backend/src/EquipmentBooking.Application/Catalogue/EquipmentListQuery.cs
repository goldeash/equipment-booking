using EquipmentBooking.Application.Common.Paging;

namespace EquipmentBooking.Application.Catalogue;

/// <summary>Filters of the catalogue search (US-03).</summary>
public sealed record EquipmentListQuery : PageQuery
{
    public string? Q { get; init; }

    public long? CategoryId { get; init; }

    public long? LocationId { get; init; }

    public long[]? FeatureIds { get; init; }

    /// <summary>Together with AvailableTo: only items free for the whole inclusive range.</summary>
    public DateOnly? AvailableFrom { get; init; }

    public DateOnly? AvailableTo { get; init; }

    /// <summary>Administrators may include retired items.</summary>
    public bool IncludeRetired { get; init; }

    /// <summary>name, inventoryNumber or value; prefix with "-" for descending order.</summary>
    public string? Sort { get; init; }
}
