using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EquipmentBooking.Application.Notifications;

/// <summary>In-app notifications (US-17, US-18, US-19).</summary>
public sealed class NotificationService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly NotificationOptions _options;

    public NotificationService(IAppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider, IOptions<NotificationOptions> options)
    {
        _db = db;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _options = options.Value;
    }

    /// <summary>Adds a notification to the unit of work; it is saved with the change that caused it.</summary>
    public void Notify(long userId, string type, long? bookingId, string title, string body) =>
        _db.Notifications.Add(new Notification(userId, type, bookingId, title, body, _options.EmailEnabled, _timeProvider.GetUtcNow()));

    public async Task<PagedResult<NotificationResponse>> GetMineAsync(MyNotificationsQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var notifications = _db.Notifications.AsNoTracking().Where(notification => notification.UserId == userId);
        if (query.UnreadOnly)
        {
            notifications = notifications.Where(notification => notification.ReadAt == null);
        }

        return await notifications
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .Select(notification => new NotificationResponse(
                notification.Id,
                notification.NotificationTypeCode,
                notification.Title,
                notification.Body,
                notification.BookingId,
                notification.CreatedAt,
                notification.ReadAt))
            .ToPagedResultAsync(query, cancellationToken);
    }

    public async Task<UnreadCountResponse> GetUnreadCountAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var count = await _db.Notifications.CountAsync(notification => notification.UserId == userId && notification.ReadAt == null, cancellationToken);
        return new UnreadCountResponse(count);
    }

    public async Task MarkReadAsync(long id, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var notification = await _db.Notifications.SingleOrDefaultAsync(candidate => candidate.Id == id && candidate.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Notification", id);
        notification.MarkRead(_timeProvider.GetUtcNow());
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllReadAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var now = _timeProvider.GetUtcNow();
        var unread = await _db.Notifications.Where(notification => notification.UserId == userId && notification.ReadAt == null).ToListAsync(cancellationToken);
        foreach (var notification in unread)
        {
            notification.MarkRead(now);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
