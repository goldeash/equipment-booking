using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Validation;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Catalogue;

/// <summary>Locations, departments, features and fixed reference lists.</summary>
public sealed class ReferenceDataService
{
    private readonly IAppDbContext _db;
    private readonly IValidator<CreateNamedItemRequest> _validator;

    public ReferenceDataService(IAppDbContext db, IValidator<CreateNamedItemRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<NamedItem>> GetLocationsAsync(CancellationToken cancellationToken) =>
        await _db.Locations.AsNoTracking().OrderBy(location => location.Name)
            .Select(location => new NamedItem(location.Id, location.Name)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<NamedItem>> GetDepartmentsAsync(CancellationToken cancellationToken) =>
        await _db.Departments.AsNoTracking().OrderBy(department => department.Name)
            .Select(department => new NamedItem(department.Id, department.Name)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<NamedItem>> GetFeaturesAsync(CancellationToken cancellationToken) =>
        await _db.Features.AsNoTracking().OrderBy(feature => feature.Name)
            .Select(feature => new NamedItem(feature.Id, feature.Name)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CodeNameItem>> GetItemConditionsAsync(CancellationToken cancellationToken) =>
        await _db.ItemConditions.AsNoTracking().OrderBy(condition => condition.SortOrder)
            .Select(condition => new CodeNameItem(condition.Code, condition.Name)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CodeNameItem>> GetMaintenanceTypesAsync(CancellationToken cancellationToken) =>
        await _db.MaintenanceTypes.AsNoTracking().OrderBy(type => type.Name)
            .Select(type => new CodeNameItem(type.Code, type.Name)).ToListAsync(cancellationToken);

    public async Task<NamedItem> CreateLocationAsync(CreateNamedItemRequest request, CancellationToken cancellationToken)
    {
        await _validator.EnsureValidAsync(request, cancellationToken);
        var location = new Location(request.Name);
        _db.Locations.Add(location);
        await _db.SaveChangesAsync(cancellationToken);
        return new NamedItem(location.Id, location.Name);
    }

    public async Task<NamedItem> CreateDepartmentAsync(CreateNamedItemRequest request, CancellationToken cancellationToken)
    {
        await _validator.EnsureValidAsync(request, cancellationToken);
        var department = new Department(request.Name);
        _db.Departments.Add(department);
        await _db.SaveChangesAsync(cancellationToken);
        return new NamedItem(department.Id, department.Name);
    }

    public async Task<NamedItem> CreateFeatureAsync(CreateNamedItemRequest request, CancellationToken cancellationToken)
    {
        await _validator.EnsureValidAsync(request, cancellationToken);
        var feature = new Feature(request.Name);
        _db.Features.Add(feature);
        await _db.SaveChangesAsync(cancellationToken);
        return new NamedItem(feature.Id, feature.Name);
    }
}
