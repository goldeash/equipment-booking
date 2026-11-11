using EquipmentBooking.Domain.Common;
using EquipmentBooking.Domain.Maintenance;
using Shouldly;

namespace EquipmentBooking.Domain.Tests;

public sealed class MaintenanceTests
{
    [Fact]
    public void Open_ended_maintenance_overlaps_every_later_period()
    {
        var record = new MaintenanceRecord(1, MaintenanceTypeCodes.Repair, TestData.Today, null, true, "Lens", 1);

        record.Overlaps(new DateRange(TestData.Today.AddDays(30), TestData.Today.AddDays(31))).ShouldBeTrue();
        record.Overlaps(new DateRange(TestData.Today.AddDays(-3), TestData.Today.AddDays(-1))).ShouldBeFalse();
    }

    [Fact]
    public void Maintenance_end_must_not_be_before_start()
    {
        Should.Throw<DomainException>(() => new MaintenanceRecord(1, MaintenanceTypeCodes.Repair, TestData.Today, TestData.Today.AddDays(-1), true, "Lens", 1))
            .Code.ShouldBe(DomainErrorCodes.InvalidMaintenancePeriod);
    }
}
