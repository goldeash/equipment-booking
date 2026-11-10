using EquipmentBooking.Application.Catalogue;
using EquipmentBooking.Domain.Catalogue;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Infrastructure.Persistence;

/// <summary>
/// Matches the term with PostgreSQL full text search over name and description (GIN index
/// ix_equipment_items_search) and with partial matches on name and inventory number (trigram index).
/// </summary>
public sealed class EquipmentTextSearch : IEquipmentTextSearch
{
    private const string SearchConfiguration = "english";

    public IQueryable<EquipmentItem> Apply(IQueryable<EquipmentItem> items, string term)
    {
        var pattern = $"%{EscapeLikePattern(term)}%";
        return items.Where(item =>
            EF.Functions.ToTsVector(SearchConfiguration, item.Name + " " + (item.Description ?? string.Empty))
                .Matches(EF.Functions.PlainToTsQuery(SearchConfiguration, term))
            || EF.Functions.ILike(item.Name, pattern)
            || EF.Functions.ILike(item.InventoryNumber, pattern));
    }

    internal static string EscapeLikePattern(string term) =>
        term.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
