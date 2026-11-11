using EquipmentBooking.Application.Common.Paging;

namespace EquipmentBooking.Application.Loans;

public sealed record LoansQuery : PageQuery
{
    public string State { get; init; } = LoanStates.Active;
}
