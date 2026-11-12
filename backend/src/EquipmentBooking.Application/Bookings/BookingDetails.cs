namespace EquipmentBooking.Application.Bookings;

public sealed record BookingDetails(
    long Id,
    BookingItemInfo Item,
    long? RequesterId,
    string? RequesterName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Purpose,
    string Status,
    CancellationInfo? Cancellation,
    DecisionInfo? Decision,
    LoanInfo? Loan,
    RoutingInfo? Routing,
    DateTimeOffset CreatedAt,
    bool CanCancel,
    bool CanChangeDates);
