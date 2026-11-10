using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Application.Common.Validation;
using EquipmentBooking.Application.Notifications;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Common;
using EquipmentBooking.Domain.Maintenance;
using EquipmentBooking.Domain.Notifications;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Maintenance;

public sealed record CreateMaintenanceRequest(
    string MaintenanceTypeCode,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool TakesOutOfService,
    string Description,
    bool ConfirmCancellation);

public sealed class CreateMaintenanceRequestValidator : AbstractValidator<CreateMaintenanceRequest>
{
    public CreateMaintenanceRequestValidator()
    {
        RuleFor(request => request.MaintenanceTypeCode).NotEmpty().MaximumLength(40);
        RuleFor(request => request.EndDate).GreaterThanOrEqualTo(request => request.StartDate).When(request => request.EndDate.HasValue)
            .WithMessage("The end date must not be before the start date.");
        RuleFor(request => request.Description).NotEmpty().MaximumLength(2000);
    }
}

public sealed record MaintenanceRecordResponse(
    long Id,
    string MaintenanceTypeCode,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool TakesOutOfService,
    string Description,
    string RecordedByName,
    DateTimeOffset CreatedAt);

public sealed record AffectedBooking(long BookingId, DateOnly StartDate, DateOnly EndDate, string Status, string? RequesterName);

/// <summary>Maintenance records; out of service periods cancel affected requests after confirmation (US-30, US-31).</summary>
public sealed class MaintenanceService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly NotificationService _notifications;
    private readonly IValidator<CreateMaintenanceRequest> _validator;

    public MaintenanceService(IAppDbContext db, ICurrentUser currentUser, NotificationService notifications, IValidator<CreateMaintenanceRequest> validator)
    {
        _db = db;
        _currentUser = currentUser;
        _notifications = notifications;
        _validator = validator;
    }

    public async Task<IReadOnlyList<MaintenanceRecordResponse>> GetAsync(long itemId, CancellationToken cancellationToken)
    {
        await EnsureItemExistsAsync(itemId, cancellationToken);
        return await _db.MaintenanceRecords.AsNoTracking()
            .Where(record => record.EquipmentItemId == itemId)
            .OrderByDescending(record => record.StartDate)
            .Select(record => new MaintenanceRecordResponse(
                record.Id,
                record.MaintenanceTypeCode,
                record.StartDate,
                record.EndDate,
                record.TakesOutOfService,
                record.Description,
                _db.Users.Where(user => user.Id == record.RecordedByUserId).Select(user => user.FullName).First(),
                record.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<MaintenanceRecordResponse> CreateAsync(long itemId, CreateMaintenanceRequest request, CancellationToken cancellationToken)
    {
        await _validator.EnsureValidAsync(request, cancellationToken);
        await EnsureItemExistsAsync(itemId, cancellationToken);
        if (!await _db.MaintenanceTypes.AnyAsync(type => type.Code == request.MaintenanceTypeCode, cancellationToken))
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure(nameof(request.MaintenanceTypeCode), "The maintenance type does not exist.")]);
        }

        var administratorId = _currentUser.UserId;
        var recordId = await _db.ExecuteInTransactionAsync(async ct =>
        {
            await _db.LockEquipmentItemAsync(itemId, ct);
            var record = new MaintenanceRecord(
                itemId,
                request.MaintenanceTypeCode,
                request.StartDate,
                request.EndDate,
                request.TakesOutOfService,
                request.Description,
                administratorId);

            if (request.TakesOutOfService)
            {
                await HandleAffectedBookingsAsync(itemId, record, request.ConfirmCancellation, ct);
            }

            _db.MaintenanceRecords.Add(record);
            await _db.SaveChangesAsync(ct);
            return record.Id;
        }, cancellationToken);

        var records = await GetAsync(itemId, cancellationToken);
        return records.Single(record => record.Id == recordId);
    }

    private async Task HandleAffectedBookingsAsync(long itemId, MaintenanceRecord record, bool confirmCancellation, CancellationToken cancellationToken)
    {
        var lastDay = record.EndDate ?? DateOnly.MaxValue;
        var affected = await _db.Bookings
            .Include(booking => booking.EquipmentItem)
            .Where(booking =>
                booking.EquipmentItemId == itemId
                && BookingStatusCodes.Blocking.Contains(booking.StatusCode)
                && booking.StartDate <= lastDay
                && booking.EndDate >= record.StartDate)
            .OrderBy(booking => booking.StartDate)
            .ToListAsync(cancellationToken);
        if (affected.Count == 0)
        {
            return;
        }

        if (affected.Any(booking => BookingStatusCodes.OnLoan.Contains(booking.StatusCode)))
        {
            throw new BusinessRuleException(
                DomainErrorCodes.InvalidMaintenancePeriod,
                "The item is on loan in this period. Record the return first or choose later dates.");
        }

        if (!confirmCancellation)
        {
            var requesterIds = affected.Select(booking => booking.RequesterId).ToList();
            var names = await _db.Users.Where(user => requesterIds.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, user => user.FullName, cancellationToken);
            var details = affected.Select(booking => new AffectedBooking(
                booking.Id,
                booking.StartDate,
                booking.EndDate,
                booking.StatusCode,
                booking.RequesterId is { } id ? names.GetValueOrDefault(id) : null)).ToList();
            throw new ConflictException(
                ErrorCodes.ConfirmationRequired,
                "Bookings exist in the maintenance period. Confirm to cancel them and notify the employees.",
                new Dictionary<string, object?> { ["affectedBookings"] = details });
        }

        foreach (var booking in affected)
        {
            booking.Cancel();
            if (booking.RequesterId is { } requesterId)
            {
                _notifications.Notify(
                    requesterId,
                    NotificationTypeCodes.BookingCancelledByMaintenance,
                    booking.Id,
                    "Booking cancelled",
                    $"Your booking of {booking.EquipmentItem!.Name} ({booking.StartDate:yyyy-MM-dd} to {booking.EndDate:yyyy-MM-dd}) was cancelled because the item is out of service for maintenance.");
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureItemExistsAsync(long itemId, CancellationToken cancellationToken)
    {
        if (!await _db.EquipmentItems.AnyAsync(item => item.Id == itemId, cancellationToken))
        {
            throw new NotFoundException("Equipment item", itemId);
        }
    }
}
