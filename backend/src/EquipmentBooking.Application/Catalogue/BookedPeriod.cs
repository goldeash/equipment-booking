namespace EquipmentBooking.Application.Catalogue;

/// <summary>
/// Period in which the item is not free; never contains the names of other employees (US-04).
/// An overdue loan and a maintenance period without a known end have no end date: the item stays
/// blocked until it is returned or the maintenance ends.
/// </summary>
public sealed record BookedPeriod(DateOnly StartDate, DateOnly? EndDate, string Kind)
{
    public const string BookingKind = "booking";
    public const string MaintenanceKind = "maintenance";
    public const string OverdueKind = "overdue";
}
