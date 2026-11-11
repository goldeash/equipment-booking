using EquipmentBooking.Application.Common.Paging;

namespace EquipmentBooking.Application.Approval;

public sealed record AssessmentLogQuery : PageQuery
{
    public string? Outcome { get; init; }

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }
}
