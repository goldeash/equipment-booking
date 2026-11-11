using EquipmentBooking.Application.Bookings;
using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Common.Time;
using EquipmentBooking.Application.Common.Validation;
using EquipmentBooking.Domain.Bookings;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Loans;

/// <summary>Issue and return of items by administrators (US-13, US-14).</summary>
public sealed class LoanService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly OrganisationCalendar _calendar;
    private readonly BookingReader _reader;
    private readonly IValidator<ReturnLoanRequest> _returnValidator;
    private readonly IValidator<LoansQuery> _queryValidator;

    public LoanService(
        IAppDbContext db,
        ICurrentUser currentUser,
        OrganisationCalendar calendar,
        BookingReader reader,
        IValidator<ReturnLoanRequest> returnValidator,
        IValidator<LoansQuery> queryValidator)
    {
        _db = db;
        _currentUser = currentUser;
        _calendar = calendar;
        _reader = reader;
        _returnValidator = returnValidator;
        _queryValidator = queryValidator;
    }

    /// <summary>Hands out the item of an approved booking from its start date; the due time is fixed now.</summary>
    public async Task<BookingDetails> IssueAsync(long bookingId, CancellationToken cancellationToken)
    {
        var administratorId = _currentUser.UserId;
        var settings = await _calendar.GetSettingsAsync(cancellationToken);
        var today = OrganisationCalendar.ToLocalDate(_calendar.Now, settings.TimeZone);
        var itemId = await _db.Bookings.Where(booking => booking.Id == bookingId).Select(booking => (long?)booking.EquipmentItemId)
            .SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Booking", bookingId);

        await _db.ExecuteInTransactionAsync(async ct =>
        {
            await _db.LockEquipmentItemAsync(itemId, ct);
            var itemStillOut = await _db.Bookings.AnyAsync(
                booking => booking.EquipmentItemId == itemId && booking.Id != bookingId && BookingStatusCodes.OnLoan.Contains(booking.StatusCode),
                ct);
            if (itemStillOut)
            {
                throw new BusinessRuleException(ErrorCodes.ItemStillOnLoan, "The item has not been returned from the previous loan yet.");
            }

            var booking = await _db.Bookings.SingleAsync(candidate => candidate.Id == bookingId, ct);
            booking.Issue(administratorId, today, OrganisationCalendar.GetDueAt(booking.EndDate, settings), _calendar.Now);
            await _db.SaveChangesAsync(ct);
            return true;
        }, cancellationToken);

        return await _reader.GetAsync(bookingId, cancellationToken);
    }

    /// <summary>Records the return and the condition; a damaged item needs a comment.</summary>
    public async Task<BookingDetails> ReturnAsync(long bookingId, ReturnLoanRequest request, CancellationToken cancellationToken)
    {
        await _returnValidator.EnsureValidAsync(request, cancellationToken);
        var administratorId = _currentUser.UserId;
        var booking = await _db.Bookings
            .Include(candidate => candidate.Loan)
            .Include(candidate => candidate.EquipmentItem)
            .SingleOrDefaultAsync(candidate => candidate.Id == bookingId, cancellationToken)
            ?? throw new NotFoundException("Booking", bookingId);

        booking.Return(administratorId, request.ConditionCode, request.Comment, _calendar.Now);
        booking.EquipmentItem!.ChangeCondition(request.ConditionCode);
        await _db.SaveChangesAsync(cancellationToken);
        return await _reader.GetAsync(bookingId, cancellationToken);
    }

    public async Task<PagedResult<LoanListItem>> GetAsync(LoansQuery query, CancellationToken cancellationToken)
    {
        await _queryValidator.EnsureValidAsync(query, cancellationToken);
        var today = await _calendar.GetTodayAsync(cancellationToken);
        var bookings = _db.Bookings.AsNoTracking();
        bookings = query.State switch
        {
            LoanStates.ReadyToIssue => bookings.Where(booking => booking.StatusCode == BookingStatusCodes.Approved && booking.StartDate <= today),
            LoanStates.Overdue => bookings.Where(booking => booking.StatusCode == BookingStatusCodes.Overdue),
            _ => bookings.Where(booking => BookingStatusCodes.OnLoan.Contains(booking.StatusCode)),
        };

        return await bookings
            .OrderBy(booking => booking.Loan == null ? booking.StartDate : booking.EndDate)
            .ThenBy(booking => booking.Id)
            .Select(booking => new LoanListItem(
                booking.Id,
                new BookingItemInfo(booking.EquipmentItemId, booking.EquipmentItem!.InventoryNumber, booking.EquipmentItem.Name, booking.EquipmentItem.Category!.Name),
                _db.Users.Where(user => user.Id == booking.RequesterId).Select(user => user.FullName).FirstOrDefault(),
                booking.StartDate,
                booking.EndDate,
                booking.StatusCode,
                booking.Loan == null ? null : booking.Loan.IssuedAt,
                booking.Loan == null ? null : booking.Loan.DueAt))
            .ToPagedResultAsync(query, cancellationToken);
    }
}
