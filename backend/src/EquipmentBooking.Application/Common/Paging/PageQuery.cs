namespace EquipmentBooking.Application.Common.Paging;

/// <summary>Page number (from 1) and page size (1 to 100) of a list request.</summary>
public record PageQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = DefaultPageSize;
}
