using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Notifications;

/// <summary>In-app notification of a user; also the outbox for e-mail delivery once a provider is configured.</summary>
public sealed class Notification : Entity
{
    private Notification()
    {
        NotificationTypeCode = string.Empty;
        Title = string.Empty;
        Body = string.Empty;
        EmailStatus = string.Empty;
    }

    public Notification(long userId, string notificationTypeCode, long? bookingId, string title, string body, bool emailEnabled, DateTimeOffset now)
    {
        UserId = userId;
        NotificationTypeCode = notificationTypeCode;
        BookingId = bookingId;
        Title = title;
        Body = body;
        CreatedAt = now;
        EmailStatus = emailEnabled ? EmailStatuses.Pending : EmailStatuses.NotRequired;
    }

    public long UserId { get; private set; }

    public string NotificationTypeCode { get; private set; }

    public long? BookingId { get; private set; }

    public string Title { get; private set; }

    public string Body { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    public string EmailStatus { get; private set; }

    public int EmailAttempts { get; private set; }

    public DateTimeOffset? EmailSentAt { get; private set; }

    public void MarkRead(DateTimeOffset now) => ReadAt ??= now;

    public void MarkEmailSent(DateTimeOffset now)
    {
        EmailAttempts++;
        EmailStatus = EmailStatuses.Sent;
        EmailSentAt = now;
    }

    public void MarkEmailFailed()
    {
        EmailAttempts++;
        EmailStatus = EmailStatuses.Failed;
    }
}
