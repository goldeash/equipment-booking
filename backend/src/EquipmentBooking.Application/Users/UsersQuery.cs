using EquipmentBooking.Application.Common.Paging;

namespace EquipmentBooking.Application.Users;

public sealed record UsersQuery : PageQuery
{
    public string? Q { get; init; }

    public string? Role { get; init; }

    public long? DepartmentId { get; init; }
}
