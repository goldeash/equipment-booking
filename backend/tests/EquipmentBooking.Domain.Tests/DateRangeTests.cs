using EquipmentBooking.Domain.Common;
using Shouldly;

namespace EquipmentBooking.Domain.Tests;

public sealed class DateRangeTests
{
    [Fact]
    public void Days_counts_both_ends()
    {
        new DateRange(new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 12)).Days.ShouldBe(3);
    }

    [Fact]
    public void End_before_start_is_rejected()
    {
        Should.Throw<DomainException>(() => new DateRange(new DateOnly(2026, 11, 12), new DateOnly(2026, 11, 10)))
            .Code.ShouldBe(DomainErrorCodes.InvalidDateRange);
    }

    [Theory]
    [InlineData(10, 12, 12, 14, true)]
    [InlineData(10, 12, 13, 14, false)]
    [InlineData(10, 20, 12, 13, true)]
    [InlineData(15, 16, 10, 14, false)]
    public void Overlaps_detects_shared_days(int startA, int endA, int startB, int endB, bool expected)
    {
        var first = new DateRange(new DateOnly(2026, 11, startA), new DateOnly(2026, 11, endA));
        var second = new DateRange(new DateOnly(2026, 11, startB), new DateOnly(2026, 11, endB));

        first.Overlaps(second).ShouldBe(expected);
        first.Contains(new DateOnly(2026, 11, startA)).ShouldBeTrue();
    }
}
