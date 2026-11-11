using EquipmentBooking.Domain.Catalogue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class ItemConditionConfiguration() : ReferenceDataConfiguration<ItemCondition>(
    "item_conditions",
    "Physical condition of an item, ordered from best to worst.")
{
    public override void Configure(EntityTypeBuilder<ItemCondition> builder)
    {
        base.Configure(builder);
        builder.Property(item => item.SortOrder).HasComment("Display order, 1 is the best condition.");
    }

    protected override IEnumerable<ItemCondition> SeedData() =>
    [
        new(ItemConditionCodes.New, "New", 1),
        new(ItemConditionCodes.Good, "Good", 2),
        new(ItemConditionCodes.Fair, "Fair", 3),
        new(ItemConditionCodes.Damaged, "Damaged", 4),
    ];
}
