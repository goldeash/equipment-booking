using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Application.Common.Time;
using EquipmentBooking.Application.Common.Validation;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Catalogue;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Catalogue;

/// <summary>Administration of equipment items and categories (US-01, US-02).</summary>
public sealed class CatalogueAdminService
{
    private readonly IAppDbContext _db;
    private readonly OrganisationCalendar _calendar;
    private readonly IValidator<SaveEquipmentRequest> _equipmentValidator;
    private readonly IValidator<SaveCategoryRequest> _categoryValidator;

    public CatalogueAdminService(
        IAppDbContext db,
        OrganisationCalendar calendar,
        IValidator<SaveEquipmentRequest> equipmentValidator,
        IValidator<SaveCategoryRequest> categoryValidator)
    {
        _db = db;
        _calendar = calendar;
        _equipmentValidator = equipmentValidator;
        _categoryValidator = categoryValidator;
    }

    public async Task<long> CreateEquipmentAsync(SaveEquipmentRequest request, CancellationToken cancellationToken)
    {
        await _equipmentValidator.EnsureValidAsync(request, cancellationToken);
        var features = await LoadReferencesAsync(request, cancellationToken);
        var item = new EquipmentItem(
            request.InventoryNumber,
            request.Name,
            request.Description,
            request.CategoryId,
            request.LocationId,
            request.Value,
            request.ConditionCode,
            request.IsSpecialised);
        item.ReplaceFeatures(features);
        _db.EquipmentItems.Add(item);
        await _db.SaveChangesAsync(cancellationToken);
        return item.Id;
    }

    public async Task UpdateEquipmentAsync(long id, SaveEquipmentRequest request, CancellationToken cancellationToken)
    {
        await _equipmentValidator.EnsureValidAsync(request, cancellationToken);
        var item = await _db.EquipmentItems.Include(candidate => candidate.Features)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
            ?? throw new NotFoundException("Equipment item", id);
        var features = await LoadReferencesAsync(request, cancellationToken);
        item.ChangeInventoryNumber(request.InventoryNumber);
        item.Update(request.Name, request.Description, request.CategoryId, request.LocationId, request.Value, request.IsSpecialised);
        item.ChangeCondition(request.ConditionCode);
        item.ReplaceFeatures(features);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Retires an item without future bookings; its history is kept (US-01).</summary>
    public async Task RetireEquipmentAsync(long id, CancellationToken cancellationToken)
    {
        var today = await _calendar.GetTodayAsync(cancellationToken);
        await _db.ExecuteInTransactionAsync(async ct =>
        {
            await _db.LockEquipmentItemAsync(id, ct);
            var item = await _db.EquipmentItems.SingleOrDefaultAsync(candidate => candidate.Id == id, ct)
                ?? throw new NotFoundException("Equipment item", id);
            var hasFutureBookings = await _db.Bookings.AnyAsync(
                booking => booking.EquipmentItemId == id
                    && BookingStatusCodes.Blocking.Contains(booking.StatusCode)
                    && booking.EndDate >= today,
                ct);
            if (hasFutureBookings)
            {
                throw new BusinessRuleException(
                    ErrorCodes.ItemHasFutureBookings,
                    "The item has current or future bookings. Cancel or complete them before retiring the item.");
            }

            item.Retire(_calendar.Now);
            await _db.SaveChangesAsync(ct);
            return true;
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        await _db.Categories.AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryResponse(
                category.Id,
                category.Name,
                category.Description,
                category.MaxLoanDays,
                category.UsageGuidelines,
                category.IsSpecialised,
                category.AutoApprovalEnabled))
            .ToListAsync(cancellationToken);

    public async Task<long> CreateCategoryAsync(SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        await _categoryValidator.EnsureValidAsync(request, cancellationToken);
        var category = new Category(
            request.Name,
            request.Description,
            request.MaxLoanDays,
            request.UsageGuidelines,
            request.IsSpecialised,
            request.AutoApprovalEnabled);
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);
        return category.Id;
    }

    public async Task UpdateCategoryAsync(long id, SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        await _categoryValidator.EnsureValidAsync(request, cancellationToken);
        var category = await _db.Categories.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
            ?? throw new NotFoundException("Category", id);
        category.Update(
            request.Name,
            request.Description,
            request.MaxLoanDays,
            request.UsageGuidelines,
            request.IsSpecialised,
            request.AutoApprovalEnabled);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<Feature>> LoadReferencesAsync(SaveEquipmentRequest request, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        if (!await _db.Categories.AnyAsync(category => category.Id == request.CategoryId, cancellationToken))
        {
            failures.Add(new ValidationFailure(nameof(request.CategoryId), "The category does not exist."));
        }

        if (!await _db.Locations.AnyAsync(location => location.Id == request.LocationId, cancellationToken))
        {
            failures.Add(new ValidationFailure(nameof(request.LocationId), "The location does not exist."));
        }

        var featureIds = (request.FeatureIds ?? []).Distinct().ToArray();
        var features = await _db.Features.Where(feature => featureIds.Contains(feature.Id)).ToListAsync(cancellationToken);
        if (features.Count != featureIds.Length)
        {
            failures.Add(new ValidationFailure(nameof(request.FeatureIds), "One or more features do not exist."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return features;
    }
}
