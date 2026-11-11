namespace EquipmentBooking.Application.Bookings;

public sealed record CreateBookingRequest(long EquipmentItemId, DateOnly StartDate, DateOnly EndDate, string Purpose);
