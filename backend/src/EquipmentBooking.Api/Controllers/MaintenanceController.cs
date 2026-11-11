using EquipmentBooking.Api.Common.Security;
using EquipmentBooking.Application.Maintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

/// <summary>Maintenance and repairs of an item.</summary>
[ApiController]
[Route("api/v1/equipment/{itemId:long}/maintenance-records")]
[Produces("application/json")]
[Tags("Maintenance")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed class MaintenanceController : ControllerBase
{
    private readonly MaintenanceService _service;

    public MaintenanceController(MaintenanceService service)
    {
        _service = service;
    }

    /// <summary>Lists the maintenance records of an item, newest first.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MaintenanceRecordResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<MaintenanceRecordResponse>>> Get(long itemId, CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(itemId, cancellationToken));

    /// <summary>Records maintenance. An out of service period with bookings returns 409 with "affectedBookings" until confirmed.</summary>
    [HttpPost]
    [ProducesResponseType<MaintenanceRecordResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<MaintenanceRecordResponse>> Create(long itemId, CreateMaintenanceRequest request, CancellationToken cancellationToken)
    {
        var record = await _service.CreateAsync(itemId, request, cancellationToken);
        return Created($"/api/v1/equipment/{itemId}/maintenance-records/{record.Id}", record);
    }
}
