using EquipmentBooking.Application.Common.Paging;

namespace EquipmentBooking.Application.Bookings;

public sealed record MyBookingsQuery : PageQuery
{
    /// <summary>Optional status filter, for example "approved".</summary>
    public string? Status { get; init; }
}
