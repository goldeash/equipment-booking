using EquipmentBooking.Domain.Catalogue;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class ItemStatusConfiguration() : ReferenceDataConfiguration<ItemStatus>(
    "item_statuses",
    "Stored statuses of equipment items. Out of service is derived from active maintenance records.")
{
    protected override IEnumerable<ItemStatus> SeedData() =>
    [
        new(ItemStatusCodes.Available, "Available"),
        new(ItemStatusCodes.Retired, "Retired"),
    ];
}
