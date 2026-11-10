using EquipmentBooking.Application.Bookings;
using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Common.Time;
using EquipmentBooking.Application.Common.Validation;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Catalogue;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Loans;

public sealed record ReturnLoanRequest(string ConditionCode, string? Comment);

public sealed class ReturnLoanRequestValidator : AbstractValidator<ReturnLoanRequest>
{
    public ReturnLoanRequestValidator()
    {
        RuleFor(request => request.ConditionCode).Must(code => ItemConditionCodes.All.Contains(code))
            .WithMessage($"'Condition Code' must be one of: {string.Join(", ", ItemConditionCodes.All)}.");
        RuleFor(request => request.Comment).NotEmpty().When(request => request.ConditionCode == ItemConditionCodes.Damaged)
            .WithMessage("A comment describing the damage is required.");
        RuleFor(request => request.Comment).MaximumLength(1000);
    }
}

public static class LoanStates
{
    /// <summary>Approved bookings whose start date has come and that can be issued.</summary>
    public const string ReadyToIssue = "ready";

    /// <summary>Issued or overdue loans.</summary>
    public const string Active = "active";

    public const string Overdue = "overdue";

    public static readonly IReadOnlyList<string> All = [ReadyToIssue, Active, Overdue];
}

public sealed record LoansQuery : PageQuery
{
    public string State { get; init; } = LoanStates.Active;
}

public sealed class LoansQueryValidator : AbstractValidator<LoansQuery>
{
    public LoansQueryValidator()
    {
        RuleFor(query => query.State).Must(state => LoanStates.All.Contains(state))
            .WithMessage($"'State' must be one of: {string.Join(", ", LoanStates.All)}.");
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PageQuery.MaxPageSize);
    }
}

public sealed record LoanListItem(
    long BookingId,
    BookingItemInfo Item,
    string? RequesterName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    DateTimeOffset? IssuedAt,
    DateTimeOffset? DueAt);

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
