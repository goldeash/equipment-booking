using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Common.Paging;

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
