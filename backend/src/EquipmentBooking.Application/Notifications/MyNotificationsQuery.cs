using EquipmentBooking.Application.Common.Paging;

namespace EquipmentBooking.Application.Notifications;

public sealed record MyNotificationsQuery : PageQuery
{
    public bool UnreadOnly { get; init; }
}
