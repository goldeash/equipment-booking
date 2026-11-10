using EquipmentBooking.Api.Common.Security;
using EquipmentBooking.Application.Catalogue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

/// <summary>Locations, departments, features and fixed reference lists.</summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
[Tags("Reference data")]
public sealed class ReferenceDataController : ControllerBase
{
    private readonly ReferenceDataService _service;

    public ReferenceDataController(ReferenceDataService service)
    {
        _service = service;
    }

    /// <summary>Lists the locations.</summary>
    [HttpGet("locations")]
    [ProducesResponseType<IReadOnlyList<NamedItem>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NamedItem>>> GetLocations(CancellationToken cancellationToken) =>
        Ok(await _service.GetLocationsAsync(cancellationToken));

    /// <summary>Creates a location.</summary>
    [HttpPost("locations")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType<NamedItem>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<NamedItem>> CreateLocation(CreateNamedItemRequest request, CancellationToken cancellationToken)
    {
        var location = await _service.CreateLocationAsync(request, cancellationToken);
        return Created($"/api/v1/locations/{location.Id}", location);
    }

    /// <summary>Lists the departments.</summary>
    [HttpGet("departments")]
    [ProducesResponseType<IReadOnlyList<NamedItem>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NamedItem>>> GetDepartments(CancellationToken cancellationToken) =>
        Ok(await _service.GetDepartmentsAsync(cancellationToken));

    /// <summary>Creates a department.</summary>
    [HttpPost("departments")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType<NamedItem>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<NamedItem>> CreateDepartment(CreateNamedItemRequest request, CancellationToken cancellationToken)
    {
        var department = await _service.CreateDepartmentAsync(request, cancellationToken);
        return Created($"/api/v1/departments/{department.Id}", department);
    }

    /// <summary>Lists the searchable item features.</summary>
    [HttpGet("features")]
    [ProducesResponseType<IReadOnlyList<NamedItem>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NamedItem>>> GetFeatures(CancellationToken cancellationToken) =>
        Ok(await _service.GetFeaturesAsync(cancellationToken));

    /// <summary>Creates a feature.</summary>
    [HttpPost("features")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType<NamedItem>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<NamedItem>> CreateFeature(CreateNamedItemRequest request, CancellationToken cancellationToken)
    {
        var feature = await _service.CreateFeatureAsync(request, cancellationToken);
        return Created($"/api/v1/features/{feature.Id}", feature);
    }

    /// <summary>Lists the item conditions from best to worst.</summary>
    [HttpGet("item-conditions")]
    [ProducesResponseType<IReadOnlyList<CodeNameItem>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CodeNameItem>>> GetItemConditions(CancellationToken cancellationToken) =>
        Ok(await _service.GetItemConditionsAsync(cancellationToken));

    /// <summary>Lists the maintenance types.</summary>
    [HttpGet("maintenance-types")]
    [ProducesResponseType<IReadOnlyList<CodeNameItem>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CodeNameItem>>> GetMaintenanceTypes(CancellationToken cancellationToken) =>
        Ok(await _service.GetMaintenanceTypesAsync(cancellationToken));
}
