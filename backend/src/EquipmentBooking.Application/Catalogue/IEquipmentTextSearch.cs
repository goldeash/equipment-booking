using EquipmentBooking.Domain.Catalogue;

namespace EquipmentBooking.Application.Catalogue;

/// <summary>Keyword search over items; implemented with the full text and trigram search of the database.</summary>
public interface IEquipmentTextSearch
{
    IQueryable<EquipmentItem> Apply(IQueryable<EquipmentItem> items, string term);
}
