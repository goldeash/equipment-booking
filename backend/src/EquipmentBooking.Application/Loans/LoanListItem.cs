using EquipmentBooking.Application.Bookings;

namespace EquipmentBooking.Application.Loans;

public sealed record LoanListItem(
    long BookingId,
    BookingItemInfo Item,
    string? RequesterName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    DateTimeOffset? IssuedAt,
    DateTimeOffset? DueAt);
