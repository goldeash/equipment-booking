namespace EquipmentBooking.Application.Approval;

/// <summary>Return history of a requester shown to the manager (US-12); never sent to the language model.</summary>
public sealed record RequesterHistoryResponse(long RequesterId, string RequesterName, int LoanCount, int LateReturnCount, DateTimeOffset? LastLateReturnAt);
