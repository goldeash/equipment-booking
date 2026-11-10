using EquipmentBooking.Application.Approval;
using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Common.Time;
using EquipmentBooking.Application.Common.Validation;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Bookings;

/// <summary>Booking requests of employees: create, change dates, cancel and list (US-06 to US-09).</summary>
public sealed class BookingService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly OrganisationCalendar _calendar;
    private readonly ApprovalRouter _router;
    private readonly BookingReader _reader;
    private readonly IValidator<CreateBookingRequest> _createValidator;
    private readonly IValidator<ChangeBookingDatesRequest> _changeValidator;
    private readonly IValidator<MyBookingsQuery> _queryValidator;

    public BookingService(
        IAppDbContext db,
        ICurrentUser currentUser,
        OrganisationCalendar calendar,
        ApprovalRouter router,
        BookingReader reader,
        IValidator<CreateBookingRequest> createValidator,
        IValidator<ChangeBookingDatesRequest> changeValidator,
        IValidator<MyBookingsQuery> queryValidator)
    {
        _db = db;
        _currentUser = currentUser;
        _calendar = calendar;
        _router = router;
        _reader = reader;
        _createValidator = createValidator;
        _changeValidator = changeValidator;
        _queryValidator = queryValidator;
    }

    /// <summary>
    /// Creates a request that blocks the item for its dates and runs the approval route. Overlapping
    /// requests are rejected by the database even when two employees confirm at the same moment.
    /// </summary>
    public async Task<BookingDetails> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);
        var requesterId = _currentUser.UserId;
        var period = new DateRange(request.StartDate, request.EndDate);
        var today = await _calendar.GetTodayAsync(cancellationToken);
        var settings = await _calendar.GetSettingsAsync(cancellationToken);

        var bookingId = await RunWithConflictDetailsAsync(request.EquipmentItemId, period, async ct =>
        {
            await _db.LockEquipmentItemAsync(request.EquipmentItemId, ct);
            var item = await _db.EquipmentItems.Include(candidate => candidate.Category)
                .SingleOrDefaultAsync(candidate => candidate.Id == request.EquipmentItemId, ct)
                ?? throw new NotFoundException("Equipment item", request.EquipmentItemId);

            var booking = Booking.Request(item, requesterId, period, request.Purpose, today);
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync(ct);

            await _router.RouteAsync(booking, item, item.Category!, settings, ct);
            await _db.SaveChangesAsync(ct);
            return booking.Id;
        }, cancellationToken);

        return await _reader.GetAsync(bookingId, cancellationToken);
    }

    /// <summary>Moves a requested or approved booking; the new dates are checked and routed again (US-08).</summary>
    public async Task<BookingDetails> ChangeDatesAsync(long id, ChangeBookingDatesRequest request, CancellationToken cancellationToken)
    {
        await _changeValidator.EnsureValidAsync(request, cancellationToken);
        var period = new DateRange(request.StartDate, request.EndDate);
        var today = await _calendar.GetTodayAsync(cancellationToken);
        var settings = await _calendar.GetSettingsAsync(cancellationToken);
        var itemId = await GetOwnedItemIdAsync(id, cancellationToken);

        await RunWithConflictDetailsAsync(itemId, period, async ct =>
        {
            await _db.LockEquipmentItemAsync(itemId, ct);
            var booking = await LoadOwnedAsync(id, ct);
            var item = await _db.EquipmentItems.Include(candidate => candidate.Category).SingleAsync(candidate => candidate.Id == itemId, ct);
            booking.ChangeDates(period, today, _calendar.Now);
            await _db.SaveChangesAsync(ct);

            await _router.RouteAsync(booking, item, item.Category!, settings, ct);
            await _db.SaveChangesAsync(ct);
            return booking.Id;
        }, cancellationToken, excludeBookingId: id);

        return await _reader.GetAsync(id, cancellationToken);
    }

    /// <summary>Cancels a requested or approved booking and frees its dates (US-08).</summary>
    public async Task<BookingDetails> CancelAsync(long id, CancellationToken cancellationToken)
    {
        var booking = await LoadOwnedAsync(id, cancellationToken);
        booking.Cancel();
        await _db.SaveChangesAsync(cancellationToken);
        return await _reader.GetAsync(id, cancellationToken);
    }

    public Task<BookingDetails> GetAsync(long id, CancellationToken cancellationToken) => _reader.GetAsync(id, cancellationToken);

    /// <summary>Bookings of the signed in employee with status and who decided (US-09).</summary>
    public async Task<PagedResult<BookingSummary>> GetMineAsync(MyBookingsQuery query, CancellationToken cancellationToken)
    {
        await _queryValidator.EnsureValidAsync(query, cancellationToken);
        var userId = _currentUser.UserId;
        var bookings = _db.Bookings.AsNoTracking().Where(booking => booking.RequesterId == userId);
        if (query.Status is not null)
        {
            bookings = bookings.Where(booking => booking.StatusCode == query.Status);
        }

        return await bookings
            .OrderByDescending(booking => booking.StartDate)
            .ThenByDescending(booking => booking.Id)
            .Select(booking => new BookingSummary(
                booking.Id,
                new BookingItemInfo(booking.EquipmentItemId, booking.EquipmentItem!.InventoryNumber, booking.EquipmentItem.Name, booking.EquipmentItem.Category!.Name),
                booking.StartDate,
                booking.EndDate,
                booking.StatusCode,
                booking.Decisions
                    .Where(decision => decision.SupersededAt == null)
                    .Select(decision => new DecisionInfo(
                        decision.DecisionSourceCode,
                        decision.Outcome,
                        _db.Users.Where(user => user.Id == decision.DecidedByUserId).Select(user => user.FullName).FirstOrDefault(),
                        decision.Comment,
                        decision.DecidedAt))
                    .FirstOrDefault(),
                booking.CreatedAt))
            .ToPagedResultAsync(query, cancellationToken);
    }

    private async Task<long> GetOwnedItemIdAsync(long id, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var itemId = await _db.Bookings.Where(booking => booking.Id == id && booking.RequesterId == userId)
            .Select(booking => (long?)booking.EquipmentItemId)
            .SingleOrDefaultAsync(cancellationToken);
        return itemId ?? throw new NotFoundException("Booking", id);
    }

    private async Task<Booking> LoadOwnedAsync(long id, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        return await _db.Bookings.Include(booking => booking.Decisions)
            .SingleOrDefaultAsync(booking => booking.Id == id && booking.RequesterId == userId, cancellationToken)
            ?? throw new NotFoundException("Booking", id);
    }

    /// <summary>Runs the action in a transaction and adds the conflicting dates to an overlap error.</summary>
    private async Task<long> RunWithConflictDetailsAsync(
        long itemId,
        DateRange period,
        Func<CancellationToken, Task<long>> action,
        CancellationToken cancellationToken,
        long? excludeBookingId = null)
    {
        try
        {
            return await _db.ExecuteInTransactionAsync(action, cancellationToken);
        }
        catch (ConflictException conflict) when (conflict.Code == ErrorCodes.BookingOverlap)
        {
            var conflicts = await _db.Bookings.AsNoTracking()
                .Where(booking =>
                    booking.EquipmentItemId == itemId
                    && booking.Id != excludeBookingId
                    && BookingStatusCodes.Blocking.Contains(booking.StatusCode)
                    && booking.StartDate <= period.End
                    && booking.EndDate >= period.Start)
                .OrderBy(booking => booking.StartDate)
                .Select(booking => new { booking.StartDate, booking.EndDate })
                .ToListAsync(cancellationToken);
            throw new ConflictException(
                conflict.Code,
                conflict.Message,
                new Dictionary<string, object?> { ["conflictingPeriods"] = conflicts });
        }
    }
}
