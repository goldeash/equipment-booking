using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Bookings;

/// <summary>
/// Request for an item for a date range. It blocks the item from creation until it is declined,
/// cancelled or returned, and becomes a loan when the item is issued.
/// </summary>
public sealed class Booking : AuditableEntity
{
    public const int MinPurposeLength = 3;
    public const int MaxPurposeLength = 1000;

    private readonly List<BookingDecision> _decisions = [];

    private Booking()
    {
        Purpose = string.Empty;
        StatusCode = string.Empty;
    }

    private Booking(long equipmentItemId, long requesterId, DateRange period, string purpose)
    {
        EquipmentItemId = equipmentItemId;
        RequesterId = requesterId;
        StartDate = period.Start;
        EndDate = period.End;
        Purpose = purpose;
        StatusCode = BookingStatusCodes.Requested;
    }

    public long EquipmentItemId { get; private set; }

    public EquipmentItem? EquipmentItem { get; private set; }

    /// <summary>Requester; null after the personal data of the loan was anonymised.</summary>
    public long? RequesterId { get; private set; }

    /// <summary>Random reference that replaces the requester after anonymisation.</summary>
    public string? AnonymousRef { get; private set; }

    public DateTimeOffset? AnonymisedAt { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public string Purpose { get; private set; }

    public string StatusCode { get; private set; }

    public Loan? Loan { get; private set; }

    /// <summary>Concurrency token mapped to the PostgreSQL system column xmin.</summary>
    public uint Version { get; private set; }

    public IReadOnlyCollection<BookingDecision> Decisions => _decisions;

    public DateRange Period => new(StartDate, EndDate);

    public BookingDecision? CurrentDecision => _decisions.SingleOrDefault(decision => decision.IsCurrent);

    public bool IsBlocking => BookingStatusCodes.Blocking.Contains(StatusCode);

    public static Booking Request(EquipmentItem item, long requesterId, DateRange period, string purpose, DateOnly today)
    {
        item.EnsureNotRetired();
        EnsureNotInPast(period, today);
        return new Booking(item.Id, requesterId, period, NormalisePurpose(purpose));
    }

    public void ApproveByManager(long managerId, string? comment, DateTimeOffset now)
    {
        EnsureStatus(BookingStatusCodes.Requested);
        AddDecision(BookingDecision.ApprovedByManager(managerId, string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(), now));
        StatusCode = BookingStatusCodes.Approved;
    }

    public void ApproveByAssistant(DateTimeOffset now)
    {
        EnsureStatus(BookingStatusCodes.Requested);
        AddDecision(BookingDecision.ApprovedByAssistant(now));
        StatusCode = BookingStatusCodes.Approved;
    }

    public void Decline(long managerId, string comment, DateTimeOffset now)
    {
        EnsureStatus(BookingStatusCodes.Requested);
        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new DomainException(DomainErrorCodes.DeclineCommentRequired, "A comment is required to decline a request.");
        }

        AddDecision(BookingDecision.DeclinedByManager(managerId, comment.Trim(), now));
        StatusCode = BookingStatusCodes.Declined;
    }

    public void Cancel()
    {
        EnsureStatus(BookingStatusCodes.Requested, BookingStatusCodes.Approved);
        StatusCode = BookingStatusCodes.Cancelled;
    }

    /// <summary>Moves the booking to new dates; the request goes through the approval route again.</summary>
    public void ChangeDates(DateRange period, DateOnly today, DateTimeOffset now)
    {
        EnsureStatus(BookingStatusCodes.Requested, BookingStatusCodes.Approved);
        EnsureNotInPast(period, today);
        CurrentDecision?.Supersede(now);
        StartDate = period.Start;
        EndDate = period.End;
        StatusCode = BookingStatusCodes.Requested;
    }

    public void Issue(long administratorId, DateOnly today, DateTimeOffset dueAt, DateTimeOffset now)
    {
        EnsureStatus(BookingStatusCodes.Approved);
        if (StartDate > today)
        {
            throw new DomainException(DomainErrorCodes.StartDateNotReached, "The item can be issued from the start date of the booking.");
        }

        Loan = new Loan(administratorId, now, dueAt);
        StatusCode = BookingStatusCodes.Issued;
    }

    public void MarkOverdue()
    {
        EnsureStatus(BookingStatusCodes.Issued);
        StatusCode = BookingStatusCodes.Overdue;
    }

    public void Return(long administratorId, string conditionCode, string? comment, DateTimeOffset now)
    {
        EnsureStatus(BookingStatusCodes.Issued, BookingStatusCodes.Overdue);
        Loan!.RecordReturn(administratorId, conditionCode, comment, now);
        StatusCode = BookingStatusCodes.Returned;
    }

    /// <summary>Removes the link to the requester while keeping the item history.</summary>
    public void Anonymise(string anonymousRef, DateTimeOffset now)
    {
        EnsureStatus(BookingStatusCodes.Returned);
        RequesterId = null;
        AnonymousRef = anonymousRef;
        AnonymisedAt = now;
    }

    private static void EnsureNotInPast(DateRange period, DateOnly today)
    {
        if (period.Start < today)
        {
            throw new DomainException(DomainErrorCodes.StartDateInPast, "The start date must not be in the past.");
        }
    }

    private static string NormalisePurpose(string purpose)
    {
        var trimmed = purpose.Trim();
        if (trimmed.Length is < MinPurposeLength or > MaxPurposeLength)
        {
            throw new DomainException(
                DomainErrorCodes.InvalidPurpose,
                $"The purpose must contain {MinPurposeLength} to {MaxPurposeLength} characters.");
        }

        return trimmed;
    }

    private void AddDecision(BookingDecision decision)
    {
        CurrentDecision?.Supersede(decision.DecidedAt);
        _decisions.Add(decision);
    }

    private void EnsureStatus(params string[] allowed)
    {
        if (!allowed.Contains(StatusCode))
        {
            throw new DomainException(
                DomainErrorCodes.InvalidTransition,
                $"The action is not available for a booking with the status '{StatusCode}'.");
        }
    }
}
