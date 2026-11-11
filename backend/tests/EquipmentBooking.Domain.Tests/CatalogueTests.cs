using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Common;
using Shouldly;

namespace EquipmentBooking.Domain.Tests;

public sealed class CatalogueTests
{
    [Fact]
    public void Category_needs_a_positive_loan_duration()
    {
        Should.Throw<DomainException>(() => new Category("Cameras", null, 0, "Work use", false, true))
            .Code.ShouldBe(DomainErrorCodes.InvalidLoanDuration);
    }

    [Fact]
    public void Category_trims_text_and_drops_empty_description()
    {
        var category = new Category(" Cameras ", " ", 7, " Work use ", true, false);

        category.Name.ShouldBe("Cameras");
        category.Description.ShouldBeNull();
        category.UsageGuidelines.ShouldBe("Work use");
        category.IsSpecialised.ShouldBeTrue();
        category.AutoApprovalEnabled.ShouldBeFalse();
    }

    [Fact]
    public void Item_value_must_not_be_negative()
    {
        Should.Throw<DomainException>(() => TestData.Item(value: -1m)).Code.ShouldBe(DomainErrorCodes.InvalidValue);
    }

    [Fact]
    public void Retired_item_cannot_be_changed()
    {
        var item = TestData.Item();
        item.Retire(TestData.Now);

        item.IsRetired.ShouldBeTrue();
        item.RetiredAt.ShouldBe(TestData.Now);
        Should.Throw<DomainException>(() => item.Update("New", null, 1, 1, 1m, false)).Code.ShouldBe(DomainErrorCodes.ItemRetired);
        Should.Throw<DomainException>(() => item.Retire(TestData.Now)).Code.ShouldBe(DomainErrorCodes.ItemRetired);
    }

    [Fact]
    public void Features_are_replaced()
    {
        var item = TestData.Item();
        item.ReplaceFeatures([new Feature("4K"), new Feature("Wireless")]);
        item.ReplaceFeatures([new Feature("HDMI")]);

        item.Features.Select(feature => feature.Name).ShouldBe(["HDMI"]);
    }
}
