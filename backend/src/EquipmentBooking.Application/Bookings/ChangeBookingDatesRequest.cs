namespace EquipmentBooking.Application.Bookings;

public sealed record ChangeBookingDatesRequest(DateOnly StartDate, DateOnly EndDate);
