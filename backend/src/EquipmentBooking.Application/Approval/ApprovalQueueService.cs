using EquipmentBooking.Application.Bookings;
using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Common.Validation;
using EquipmentBooking.Application.Notifications;
using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Notifications;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Approval;

/// <summary>Queue of routed requests and decisions of department managers (US-10, US-11, US-12).</summary>
public sealed class ApprovalQueueService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly BookingReader _reader;
    private readonly NotificationService _notifications;
    private readonly TimeProvider _timeProvider;
    private readonly IValidator<DecideRequest> _validator;

    public ApprovalQueueService(
        IAppDbContext db,
        ICurrentUser currentUser,
        BookingReader reader,
        NotificationService notifications,
        TimeProvider timeProvider,
        IValidator<DecideRequest> validator)
    {
        _db = db;
        _currentUser = currentUser;
        _reader = reader;
        _notifications = notifications;
        _timeProvider = timeProvider;
        _validator = validator;
    }

    /// <summary>Requests of the manager's department that were routed for a manual decision, oldest first.</summary>
    public async Task<PagedResult<ApprovalQueueItem>> GetQueueAsync(PageQuery page, CancellationToken cancellationToken)
    {
        var departmentId = _currentUser.DepartmentId;
        return await QueueQuery(departmentId)
            .OrderBy(booking => booking.CreatedAt)
            .ThenBy(booking => booking.Id)
            .Select(booking => new ApprovalQueueItem(
                booking.Id,
                new BookingItemInfo(booking.EquipmentItemId, booking.EquipmentItem!.InventoryNumber, booking.EquipmentItem.Name, booking.EquipmentItem.Category!.Name),
                booking.EquipmentItem.Value,
                booking.RequesterId!.Value,
                _db.Users.Where(user => user.Id == booking.RequesterId).Select(user => user.FullName).First(),
                booking.StartDate,
                booking.EndDate,
                booking.Purpose,
                _db.ApprovalAssessments
                    .Where(assessment => assessment.BookingId == booking.Id)
                    .OrderByDescending(assessment => assessment.AssessedAt)
                    .ThenByDescending(assessment => assessment.Id)
                    .Take(1)
                    .SelectMany(assessment => assessment.Reasons)
                    .Join(_db.RoutingReasons, reason => reason.RoutingReasonCode, reason => reason.Code, (_, reason) => new RoutingReasonInfo(reason.Code, reason.Name))
                    .ToList(),
                _db.ApprovalAssessments
                    .Where(assessment => assessment.BookingId == booking.Id)
                    .OrderByDescending(assessment => assessment.AssessedAt)
                    .ThenByDescending(assessment => assessment.Id)
                    .Select(assessment => assessment.Summary)
                    .FirstOrDefault(),
                booking.CreatedAt))
            .ToPagedResultAsync(page, cancellationToken);
    }

    public async Task<BookingDetails> DecideAsync(long bookingId, DecideRequest request, CancellationToken cancellationToken)
    {
        await _validator.EnsureValidAsync(request, cancellationToken);
        var managerId = _currentUser.UserId;
        var departmentId = _currentUser.DepartmentId;
        var isInQueue = await QueueQuery(departmentId).AnyAsync(booking => booking.Id == bookingId, cancellationToken);
        if (!isInQueue)
        {
            throw new NotFoundException("Routed request", bookingId);
        }

        var booking = await _db.Bookings
            .Include(candidate => candidate.Decisions)
            .Include(candidate => candidate.EquipmentItem)
            .SingleAsync(candidate => candidate.Id == bookingId, cancellationToken);
        var now = _timeProvider.GetUtcNow();
        var approved = request.Outcome == DecisionOutcomes.Approved;
        if (approved)
        {
            booking.ApproveByManager(managerId, request.Comment, now);
        }
        else
        {
            booking.Decline(managerId, request.Comment ?? string.Empty, now);
        }

        var managerName = await _db.Users.Where(user => user.Id == managerId).Select(user => user.FullName).SingleAsync(cancellationToken);
        var comment = string.IsNullOrWhiteSpace(request.Comment) ? string.Empty : $" Comment: {request.Comment.Trim()}";
        _notifications.Notify(
            booking.RequesterId!.Value,
            approved ? NotificationTypeCodes.RequestApproved : NotificationTypeCodes.RequestDeclined,
            booking.Id,
            approved ? "Request approved" : "Request declined",
            $"Your request for {booking.EquipmentItem!.Name} ({booking.StartDate:yyyy-MM-dd} to {booking.EndDate:yyyy-MM-dd}) was {(approved ? "approved" : "declined")} by {managerName}.{comment}");
        await _db.SaveChangesAsync(cancellationToken);
        return await _reader.GetAsync(bookingId, cancellationToken);
    }

    public async Task<RequesterHistoryResponse> GetRequesterHistoryAsync(long bookingId, CancellationToken cancellationToken)
    {
        var departmentId = _currentUser.DepartmentId;
        var requester = await _db.Bookings.AsNoTracking()
            .Where(booking => booking.Id == bookingId && booking.RequesterId != null)
            .Join(_db.Users, booking => booking.RequesterId, user => user.Id, (_, user) => user)
            .Where(user => user.DepartmentId == departmentId)
            .Select(user => new { user.Id, user.FullName })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Booking", bookingId);

        var history = await _db.RequesterReturnHistories.AsNoTracking()
            .SingleOrDefaultAsync(row => row.UserId == requester.Id, cancellationToken);
        return new RequesterHistoryResponse(
            requester.Id,
            requester.FullName,
            history?.LoanCount ?? 0,
            history?.LateReturnCount ?? 0,
            history?.LastLateReturnAt);
    }

    private IQueryable<Booking> QueueQuery(long departmentId) =>
        _db.Bookings.AsNoTracking().Where(booking =>
            booking.StatusCode == BookingStatusCodes.Requested
            && _db.Users.Any(user => user.Id == booking.RequesterId && user.DepartmentId == departmentId)
            && _db.ApprovalAssessments
                .Where(assessment => assessment.BookingId == booking.Id)
                .OrderByDescending(assessment => assessment.AssessedAt)
                .ThenByDescending(assessment => assessment.Id)
                .Select(assessment => assessment.Outcome)
                .FirstOrDefault() == AssessmentOutcomes.Routed);
}
