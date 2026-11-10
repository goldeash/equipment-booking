using EquipmentBooking.Application.Common.Time;
using EquipmentBooking.Domain.Settings;
using Shouldly;

namespace EquipmentBooking.Application.Tests.Common;

public sealed class OrganisationCalendarTests
{
    private static readonly OrganisationSettings Settings = new(500m, true, 365, "Europe/Vilnius", new TimeOnly(18, 0));

    [Fact]
    public void Due_time_uses_winter_time_of_the_organisation()
    {
        OrganisationCalendar.GetDueAt(new DateOnly(2026, 11, 12), Settings)
            .ShouldBe(new DateTimeOffset(2026, 11, 12, 16, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Due_time_uses_summer_time_of_the_organisation()
    {
        OrganisationCalendar.GetDueAt(new DateOnly(2026, 7, 1), Settings)
            .ShouldBe(new DateTimeOffset(2026, 7, 1, 15, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Local_date_changes_at_local_midnight()
    {
        var instant = new DateTimeOffset(2026, 11, 10, 22, 30, 0, TimeSpan.Zero);

        OrganisationCalendar.ToLocalDate(instant, "Europe/Vilnius").ShouldBe(new DateOnly(2026, 11, 11));
        OrganisationCalendar.ToLocalDate(instant, "UTC").ShouldBe(new DateOnly(2026, 11, 10));
    }
}
