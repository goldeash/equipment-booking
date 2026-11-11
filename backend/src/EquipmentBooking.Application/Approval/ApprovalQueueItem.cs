using EquipmentBooking.Application.Bookings;

namespace EquipmentBooking.Application.Approval;

public sealed record ApprovalQueueItem(
    long BookingId,
    BookingItemInfo Item,
    decimal ItemValue,
    long RequesterId,
    string RequesterName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Purpose,
    IReadOnlyList<RoutingReasonInfo> Reasons,
    string? Summary,
    DateTimeOffset RequestedAt);
