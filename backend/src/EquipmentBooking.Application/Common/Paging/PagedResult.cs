using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Common.Paging;

/// <summary>One page of a list with the total number of matching items.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>Page number (from 1) and page size (1 to 100) of a list request.</summary>
public record PageQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = DefaultPageSize;
}

public static class PagingExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PageQuery page,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, page.Page);
        var pageSize = Math.Clamp(page.PageSize, 1, PageQuery.MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, pageNumber, pageSize, total);
    }
}
