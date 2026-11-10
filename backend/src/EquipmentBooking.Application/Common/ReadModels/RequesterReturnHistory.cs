namespace EquipmentBooking.Application.Common.ReadModels;

/// <summary>Row of the view v_requester_return_history; anonymised loans are not counted.</summary>
public sealed class RequesterReturnHistory
{
    public long UserId { get; init; }

    public int LoanCount { get; init; }

    public int LateReturnCount { get; init; }

    public DateTimeOffset? LastLateReturnAt { get; init; }
}
