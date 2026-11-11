namespace EquipmentBooking.Application.Maintenance;

public sealed record AffectedBooking(long BookingId, DateOnly StartDate, DateOnly EndDate, string Status, string? RequesterName);
