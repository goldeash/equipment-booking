using EquipmentBooking.Domain.Common;
using EquipmentBooking.Domain.Settings;
using Shouldly;

namespace EquipmentBooking.Domain.Tests;

public sealed class SettingsTests
{
    [Fact]
    public void Value_equal_to_the_threshold_is_high_value()
    {
        var settings = new OrganisationSettings(500m, true, 365, "Europe/Vilnius", new TimeOnly(18, 0));

        settings.IsLowValue(499.99m).ShouldBeTrue();
        settings.IsLowValue(500m).ShouldBeFalse();
    }

    [Fact]
    public void Threshold_and_retention_are_validated()
    {
        var settings = new OrganisationSettings(500m, true, 365, "Europe/Vilnius", new TimeOnly(18, 0));

        Should.Throw<DomainException>(() => settings.Update(0m, true, 365, new TimeOnly(18, 0), 1)).Code.ShouldBe(DomainErrorCodes.InvalidThreshold);
        Should.Throw<DomainException>(() => settings.Update(100m, true, 29, new TimeOnly(18, 0), 1)).Code.ShouldBe(DomainErrorCodes.InvalidRetentionPeriod);
    }
}
