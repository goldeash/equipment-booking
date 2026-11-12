using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Common.Time;
using EquipmentBooking.Application.Common.Validation;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Catalogue;

/// <summary>Catalogue search, item card, booked periods and item history (US-03, US-04, US-32).</summary>
public sealed class EquipmentQueries
{
    private readonly IAppDbContext _db;
    private readonly IEquipmentTextSearch _textSearch;
    private readonly OrganisationCalendar _calendar;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<EquipmentListQuery> _listValidator;
    private readonly IValidator<BookedPeriodsQuery> _periodsValidator;

    public EquipmentQueries(
        IAppDbContext db,
        IEquipmentTextSearch textSearch,
        OrganisationCalendar calendar,
        ICurrentUser currentUser,
        IValidator<EquipmentListQuery> listValidator,
        IValidator<BookedPeriodsQuery> periodsValidator)
    {
        _db = db;
        _textSearch = textSearch;
        _calendar = calendar;
        _currentUser = currentUser;
        _listValidator = listValidator;
        _periodsValidator = periodsValidator;
    }

    public async Task<PagedResult<EquipmentListItem>> SearchAsync(EquipmentListQuery query, CancellationToken cancellationToken)
    {
        await _listValidator.EnsureValidAsync(query, cancellationToken);
        var today = await _calendar.GetTodayAsync(cancellationToken);
        var items = _db.EquipmentItems.AsNoTracking();

        if (!(query.IncludeRetired && _currentUser.IsInRole(RoleCodes.Administrator)))
        {
            items = items.Where(item => item.RetiredAt == null);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim();
            items = _textSearch.Apply(items, term);
        }

        if (query.CategoryId is { } categoryId)
        {
            items = items.Where(item => item.CategoryId == categoryId);
        }

        if (query.LocationId is { } locationId)
        {
            items = items.Where(item => item.LocationId == locationId);
        }

        foreach (var featureId in query.FeatureIds ?? [])
        {
            items = items.Where(item => item.Features.Any(feature => feature.Id == featureId));
        }

        if (query.AvailableFrom is { } from && query.AvailableTo is { } to)
        {
            items = items.Where(item =>
                item.RetiredAt == null
                && !_db.Bookings.Any(booking =>
                    booking.EquipmentItemId == item.Id
                    && BookingStatusCodes.Blocking.Contains(booking.StatusCode)
                    && booking.StartDate <= to
                    && booking.EndDate >= from)
                && !_db.Bookings.Any(booking =>
                    booking.EquipmentItemId == item.Id
                    && booking.StatusCode == BookingStatusCodes.Overdue)
                && !_db.MaintenanceRecords.Any(record =>
                    record.EquipmentItemId == item.Id
                    && record.TakesOutOfService
                    && record.StartDate <= to
                    && (record.EndDate == null || record.EndDate >= from)));
        }

        items = query.Sort switch
        {
            "-name" => items.OrderByDescending(item => item.Name).ThenBy(item => item.Id),
            "inventoryNumber" => items.OrderBy(item => item.InventoryNumber),
            "-inventoryNumber" => items.OrderByDescending(item => item.InventoryNumber),
            "value" => items.OrderBy(item => item.Value).ThenBy(item => item.Id),
            "-value" => items.OrderByDescending(item => item.Value).ThenBy(item => item.Id),
            _ => items.OrderBy(item => item.Name).ThenBy(item => item.Id),
        };

        return await items
            .Select(item => new EquipmentListItem(
                item.Id,
                item.InventoryNumber,
                item.Name,
                item.CategoryId,
                item.Category!.Name,
                item.LocationId,
                item.Location!.Name,
                item.Value,
                item.ConditionCode,
                item.RetiredAt != null
                    ? ItemStatusCodes.Retired
                    : _db.MaintenanceRecords.Any(record =>
                        record.EquipmentItemId == item.Id
                        && record.TakesOutOfService
                        && record.StartDate <= today
                        && (record.EndDate == null || record.EndDate >= today))
                        ? ItemStatusCodes.OutOfService
                        : ItemStatusCodes.Available,
                item.IsSpecialised || item.Category.IsSpecialised,
                item.Features.OrderBy(feature => feature.Name).Select(feature => feature.Name).ToList()))
            .ToPagedResultAsync(query, cancellationToken);
    }

    public async Task<EquipmentDetails> GetAsync(long id, CancellationToken cancellationToken)
    {
        var today = await _calendar.GetTodayAsync(cancellationToken);
        var details = await _db.EquipmentItems.AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new EquipmentDetails(
                item.Id,
                item.InventoryNumber,
                item.Name,
                item.Description,
                new CategorySummary(
                    item.CategoryId,
                    item.Category!.Name,
                    item.Category.MaxLoanDays,
                    item.Category.UsageGuidelines,
                    item.Category.IsSpecialised),
                new NamedItem(item.LocationId, item.Location!.Name),
                item.Value,
                item.ConditionCode,
                item.RetiredAt != null
                    ? ItemStatusCodes.Retired
                    : _db.MaintenanceRecords.Any(record =>
                        record.EquipmentItemId == item.Id
                        && record.TakesOutOfService
                        && record.StartDate <= today
                        && (record.EndDate == null || record.EndDate >= today))
                        ? ItemStatusCodes.OutOfService
                        : ItemStatusCodes.Available,
                item.IsSpecialised,
                item.Features.OrderBy(feature => feature.Name).Select(feature => new NamedItem(feature.Id, feature.Name)).ToList(),
                item.RetiredAt))
            .SingleOrDefaultAsync(cancellationToken);

        if (details is null || (details.Status == ItemStatusCodes.Retired && !_currentUser.IsInRole(RoleCodes.Administrator)))
        {
            throw new NotFoundException("Equipment item", id);
        }

        return details;
    }

    public async Task<IReadOnlyList<BookedPeriod>> GetBookedPeriodsAsync(long id, BookedPeriodsQuery query, CancellationToken cancellationToken)
    {
        await _periodsValidator.EnsureValidAsync(query, cancellationToken);
        if (!await _db.EquipmentItems.AnyAsync(item => item.Id == id, cancellationToken))
        {
            throw new NotFoundException("Equipment item", id);
        }

        var bookings = await _db.Bookings.AsNoTracking()
            .Where(booking =>
                booking.EquipmentItemId == id
                && BookingStatusCodes.Blocking.Contains(booking.StatusCode)
                && booking.StartDate <= query.To
                && (booking.EndDate >= query.From || booking.StatusCode == BookingStatusCodes.Overdue))
            .Select(booking => booking.StatusCode == BookingStatusCodes.Overdue
                ? new BookedPeriod(booking.StartDate, null, BookedPeriod.OverdueKind)
                : new BookedPeriod(booking.StartDate, booking.EndDate, BookedPeriod.BookingKind))
            .ToListAsync(cancellationToken);
        var maintenance = await _db.MaintenanceRecords.AsNoTracking()
            .Where(record =>
                record.EquipmentItemId == id
                && record.TakesOutOfService
                && record.StartDate <= query.To
                && (record.EndDate == null || record.EndDate >= query.From))
            .Select(record => new BookedPeriod(record.StartDate, record.EndDate, BookedPeriod.MaintenanceKind))
            .ToListAsync(cancellationToken);

        return [.. bookings.Concat(maintenance).OrderBy(period => period.StartDate)];
    }

    public async Task<IReadOnlyList<EquipmentHistoryEvent>> GetHistoryAsync(long id, CancellationToken cancellationToken)
    {
        if (!await _db.EquipmentItems.AnyAsync(item => item.Id == id, cancellationToken))
        {
            throw new NotFoundException("Equipment item", id);
        }

        var loans = await _db.Bookings.AsNoTracking()
            .Where(booking => booking.EquipmentItemId == id && booking.Loan != null)
            .Select(booking => new
            {
                booking.Id,
                booking.StartDate,
                booking.EndDate,
                RequesterName = _db.Users.Where(user => user.Id == booking.RequesterId).Select(user => user.FullName).FirstOrDefault(),
                booking.AnonymousRef,
                booking.Loan!.IssuedAt,
                booking.Loan.ReturnedAt,
                booking.Loan.ReturnConditionCode,
            })
            .ToListAsync(cancellationToken);
        var maintenance = await _db.MaintenanceRecords.AsNoTracking()
            .Where(record => record.EquipmentItemId == id)
            .Select(record => new { record.CreatedAt, record.MaintenanceTypeCode, record.StartDate, record.EndDate, record.Description })
            .ToListAsync(cancellationToken);
        var conditions = await _db.EquipmentConditionChanges.AsNoTracking()
            .Where(change => change.EquipmentItemId == id)
            .Select(change => new { change.ChangedAt, change.FromConditionCode, change.ToConditionCode })
            .ToListAsync(cancellationToken);

        var events = new List<EquipmentHistoryEvent>();
        foreach (var loan in loans)
        {
            var holder = loan.RequesterName ?? $"anonymised ({loan.AnonymousRef})";
            events.Add(new EquipmentHistoryEvent(loan.IssuedAt, "issued", $"Issued to {holder} for {loan.StartDate:yyyy-MM-dd} to {loan.EndDate:yyyy-MM-dd}", loan.Id));
            if (loan.ReturnedAt is { } returnedAt)
            {
                events.Add(new EquipmentHistoryEvent(returnedAt, "returned", $"Returned by {holder} in condition {loan.ReturnConditionCode}", loan.Id));
            }
        }

        events.AddRange(maintenance.Select(record => new EquipmentHistoryEvent(
            record.CreatedAt,
            "maintenance",
            $"{record.MaintenanceTypeCode} from {record.StartDate:yyyy-MM-dd}{(record.EndDate is { } end ? $" to {end:yyyy-MM-dd}" : string.Empty)}: {record.Description}",
            null)));
        events.AddRange(conditions.Select(change => new EquipmentHistoryEvent(
            change.ChangedAt,
            "condition",
            change.FromConditionCode is null ? $"Condition set to {change.ToConditionCode}" : $"Condition changed from {change.FromConditionCode} to {change.ToConditionCode}",
            null)));

        return [.. events.OrderByDescending(item => item.OccurredAt)];
    }

}
