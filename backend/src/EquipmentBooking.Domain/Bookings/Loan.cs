using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Bookings;

/// <summary>Issue and return of the item of an approved booking. One loan per booking.</summary>
public sealed class Loan
{
    private Loan()
    {
    }

    internal Loan(long issuedByUserId, DateTimeOffset issuedAt, DateTimeOffset dueAt)
    {
        IssuedByUserId = issuedByUserId;
        IssuedAt = issuedAt;
        DueAt = dueAt;
    }

    public long BookingId { get; private set; }

    public DateTimeOffset IssuedAt { get; private set; }

    public long IssuedByUserId { get; private set; }

    /// <summary>Due time fixed when the item is issued; later settings changes do not move it.</summary>
    public DateTimeOffset DueAt { get; private set; }

    public DateTimeOffset? ReturnedAt { get; private set; }

    public long? ReceivedByUserId { get; private set; }

    public string? ReturnConditionCode { get; private set; }

    public string? ReturnComment { get; private set; }

    internal void RecordReturn(long receivedByUserId, string conditionCode, string? comment, DateTimeOffset now)
    {
        var trimmedComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (conditionCode == ItemConditionCodes.Damaged && trimmedComment is null)
        {
            throw new DomainException(DomainErrorCodes.DamageCommentRequired, "A comment describing the damage is required.");
        }

        ReceivedByUserId = receivedByUserId;
        ReturnConditionCode = conditionCode;
        ReturnComment = trimmedComment;
        ReturnedAt = now;
    }
}
