namespace EquipmentBooking.Application.Bookings;

public sealed record BookingSummary(
    long Id,
    BookingItemInfo Item,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    DecisionInfo? Decision,
    DateTimeOffset CreatedAt);
