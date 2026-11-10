using EquipmentBooking.Application.Auth;
using EquipmentBooking.Application.Bookings;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

/// <summary>Profile, bookings and notifications of the signed in user.</summary>
[ApiController]
[Route("api/v1/me")]
[Produces("application/json")]
[Tags("Current user")]
public sealed class MeController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly BookingService _bookings;
    private readonly NotificationService _notifications;

    public MeController(AuthService authService, BookingService bookings, NotificationService notifications)
    {
        _authService = authService;
        _bookings = bookings;
        _notifications = notifications;
    }

    /// <summary>Returns the profile of the signed in user.</summary>
    [HttpGet]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> Get(CancellationToken cancellationToken) =>
        Ok(await _authService.GetCurrentUserAsync(cancellationToken));

    /// <summary>Records that the user has seen the privacy notice (acknowledgement, not consent).</summary>
    [HttpPut("privacy-notice-acknowledgement")]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> AcknowledgePrivacyNotice(CancellationToken cancellationToken) =>
        Ok(await _authService.AcknowledgePrivacyNoticeAsync(cancellationToken));

    /// <summary>Lists the bookings of the signed in employee with status and who decided.</summary>
    [HttpGet("bookings")]
    [ProducesResponseType<PagedResult<BookingSummary>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<BookingSummary>>> GetBookings([FromQuery] MyBookingsQuery query, CancellationToken cancellationToken) =>
        Ok(await _bookings.GetMineAsync(query, cancellationToken));

    /// <summary>Lists the notifications of the signed in user, newest first.</summary>
    [HttpGet("notifications")]
    [ProducesResponseType<PagedResult<NotificationResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NotificationResponse>>> GetNotifications([FromQuery] MyNotificationsQuery query, CancellationToken cancellationToken) =>
        Ok(await _notifications.GetMineAsync(query, cancellationToken));

    /// <summary>Returns the number of unread notifications.</summary>
    [HttpGet("notifications/unread-count")]
    [ProducesResponseType<UnreadCountResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UnreadCountResponse>> GetUnreadCount(CancellationToken cancellationToken) =>
        Ok(await _notifications.GetUnreadCountAsync(cancellationToken));

    /// <summary>Marks a notification as read.</summary>
    [HttpPut("notifications/{id:long}/read-state")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(long id, CancellationToken cancellationToken)
    {
        await _notifications.MarkReadAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Marks all notifications as read.</summary>
    [HttpPut("notifications/read-state")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await _notifications.MarkAllReadAsync(cancellationToken);
        return NoContent();
    }
}
