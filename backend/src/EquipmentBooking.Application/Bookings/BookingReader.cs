using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Bookings;

/// <summary>Builds booking views and enforces who may see a booking.</summary>
public sealed class BookingReader
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public BookingReader(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>The requester, the manager of the requester's department and administrators may see a booking.</summary>
    public async Task<BookingDetails> GetAsync(long id, CancellationToken cancellationToken)
    {
        var row = await _db.Bookings.AsNoTracking()
            .Where(booking => booking.Id == id)
            .Select(booking => new
            {
                booking.Id,
                Item = new BookingItemInfo(booking.EquipmentItemId, booking.EquipmentItem!.InventoryNumber, booking.EquipmentItem.Name, booking.EquipmentItem.Category!.Name),
                booking.RequesterId,
                Requester = _db.Users.Where(user => user.Id == booking.RequesterId).Select(user => new { user.FullName, user.DepartmentId }).FirstOrDefault(),
                booking.StartDate,
                booking.EndDate,
                booking.Purpose,
                booking.StatusCode,
                booking.CreatedAt,
                Decision = booking.Decisions
                    .Where(decision => decision.SupersededAt == null)
                    .Select(decision => new DecisionInfo(
                        decision.DecisionSourceCode,
                        decision.Outcome,
                        _db.Users.Where(user => user.Id == decision.DecidedByUserId).Select(user => user.FullName).FirstOrDefault(),
                        decision.Comment,
                        decision.DecidedAt))
                    .FirstOrDefault(),
                Loan = booking.Loan == null
                    ? null
                    : new LoanInfo(booking.Loan.IssuedAt, booking.Loan.DueAt, booking.Loan.ReturnedAt, booking.Loan.ReturnConditionCode, booking.Loan.ReturnComment),
                Routing = _db.ApprovalAssessments
                    .Where(assessment => assessment.BookingId == booking.Id)
                    .OrderByDescending(assessment => assessment.AssessedAt)
                    .ThenByDescending(assessment => assessment.Id)
                    .Select(assessment => new RoutingInfo(
                        assessment.Outcome,
                        assessment.PurposeResult,
                        assessment.Reasons.Select(reason => reason.RoutingReasonCode).ToList(),
                        assessment.Summary,
                        assessment.AssessedAt))
                    .FirstOrDefault(),
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Booking", id);

        var isOwner = row.RequesterId == _currentUser.UserIdOrNull;
        var isDepartmentManager = _currentUser.IsInRole(RoleCodes.Manager) && row.Requester?.DepartmentId == _currentUser.DepartmentId;
        var isAdministrator = _currentUser.IsInRole(RoleCodes.Administrator);
        if (!isOwner && !isDepartmentManager && !isAdministrator)
        {
            throw new NotFoundException("Booking", id);
        }

        var editable = isOwner && (row.StatusCode == BookingStatusCodes.Requested || row.StatusCode == BookingStatusCodes.Approved);
        return new BookingDetails(
            row.Id,
            row.Item,
            row.RequesterId,
            row.Requester?.FullName,
            row.StartDate,
            row.EndDate,
            row.Purpose,
            row.StatusCode,
            row.Decision,
            row.Loan,
            row.Routing,
            row.CreatedAt,
            CanCancel: editable,
            CanChangeDates: editable);
    }
}
