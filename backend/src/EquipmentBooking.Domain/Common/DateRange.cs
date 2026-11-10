namespace EquipmentBooking.Domain.Common;

/// <summary>Inclusive range of calendar dates.</summary>
public readonly record struct DateRange
{
    public DateRange(DateOnly start, DateOnly end)
    {
        if (end < start)
        {
            throw new DomainException(DomainErrorCodes.InvalidDateRange, "The end date must not be before the start date.");
        }

        Start = start;
        End = end;
    }

    public DateOnly Start { get; }

    public DateOnly End { get; }

    /// <summary>Number of calendar days covered, counting both ends.</summary>
    public int Days => End.DayNumber - Start.DayNumber + 1;

    public bool Overlaps(DateRange other) => Start <= other.End && other.Start <= End;

    public bool Contains(DateOnly date) => date >= Start && date <= End;
}
