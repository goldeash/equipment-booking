using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Bookings;

/// <summary>
/// Decision of a manager or the AI assistant on a request. A booking can have several decisions
/// when its dates are changed; only the decision without <see cref="SupersededAt"/> is current.
/// </summary>
public sealed class BookingDecision : Entity
{
    private BookingDecision()
    {
        DecisionSourceCode = string.Empty;
        Outcome = string.Empty;
    }

    private BookingDecision(string decisionSourceCode, long? decidedByUserId, string outcome, string? comment, DateTimeOffset decidedAt)
    {
        DecisionSourceCode = decisionSourceCode;
        DecidedByUserId = decidedByUserId;
        Outcome = outcome;
        Comment = comment;
        DecidedAt = decidedAt;
    }

    public long BookingId { get; private set; }

    public string DecisionSourceCode { get; private set; }

    public long? DecidedByUserId { get; private set; }

    public string Outcome { get; private set; }

    public string? Comment { get; private set; }

    public DateTimeOffset DecidedAt { get; private set; }

    public DateTimeOffset? SupersededAt { get; private set; }

    public bool IsCurrent => SupersededAt is null;

    internal static BookingDecision ApprovedByManager(long managerId, string? comment, DateTimeOffset now) =>
        new(DecisionSourceCodes.Manager, managerId, DecisionOutcomes.Approved, comment, now);

    internal static BookingDecision DeclinedByManager(long managerId, string comment, DateTimeOffset now) =>
        new(DecisionSourceCodes.Manager, managerId, DecisionOutcomes.Declined, comment, now);

    internal static BookingDecision ApprovedByAssistant(DateTimeOffset now) =>
        new(DecisionSourceCodes.AiAssistant, null, DecisionOutcomes.Approved, null, now);

    internal void Supersede(DateTimeOffset now) => SupersededAt ??= now;
}
