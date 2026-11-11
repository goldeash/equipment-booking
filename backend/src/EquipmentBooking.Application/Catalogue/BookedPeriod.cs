namespace EquipmentBooking.Application.Catalogue;

/// <summary>Period in which the item is not free; never contains the names of other employees (US-04).</summary>
public sealed record BookedPeriod(DateOnly StartDate, DateOnly? EndDate, string Kind)
{
    public const string BookingKind = "booking";
    public const string MaintenanceKind = "maintenance";
}
