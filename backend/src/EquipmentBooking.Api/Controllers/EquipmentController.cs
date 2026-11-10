using EquipmentBooking.Api.Common.Security;
using EquipmentBooking.Application.Catalogue;
using EquipmentBooking.Application.Common.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

/// <summary>Equipment catalogue: search, item card, booked periods and administration.</summary>
[ApiController]
[Route("api/v1/equipment")]
[Produces("application/json")]
[Tags("Equipment")]
public sealed class EquipmentController : ControllerBase
{
    private readonly EquipmentQueries _queries;
    private readonly CatalogueAdminService _adminService;

    public EquipmentController(EquipmentQueries queries, CatalogueAdminService adminService)
    {
        _queries = queries;
        _adminService = adminService;
    }

    /// <summary>Searches the catalogue by keyword and filters it by category, location, features and availability.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<EquipmentListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<EquipmentListItem>>> Search([FromQuery] EquipmentListQuery query, CancellationToken cancellationToken) =>
        Ok(await _queries.SearchAsync(query, cancellationToken));

    /// <summary>Returns the card of an item.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType<EquipmentDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentDetails>> Get(long id, CancellationToken cancellationToken) =>
        Ok(await _queries.GetAsync(id, cancellationToken));

    /// <summary>Returns the periods in which the item is booked or out of service, without names of employees.</summary>
    [HttpGet("{id:long}/booked-periods")]
    [ProducesResponseType<IReadOnlyList<BookedPeriod>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<BookedPeriod>>> GetBookedPeriods(
        long id,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken) =>
        Ok(await _queries.GetBookedPeriodsAsync(id, new BookedPeriodsQuery(from, to), cancellationToken));

    /// <summary>Returns the loans, condition changes and maintenance of an item in date order.</summary>
    [HttpGet("{id:long}/history")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType<IReadOnlyList<EquipmentHistoryEvent>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<EquipmentHistoryEvent>>> GetHistory(long id, CancellationToken cancellationToken) =>
        Ok(await _queries.GetHistoryAsync(id, cancellationToken));

    /// <summary>Adds an item to the catalogue with the status Available.</summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType<EquipmentDetails>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EquipmentDetails>> Create(SaveEquipmentRequest request, CancellationToken cancellationToken)
    {
        var id = await _adminService.CreateEquipmentAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, await _queries.GetAsync(id, cancellationToken));
    }

    /// <summary>Updates an item.</summary>
    [HttpPut("{id:long}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType<EquipmentDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EquipmentDetails>> Update(long id, SaveEquipmentRequest request, CancellationToken cancellationToken)
    {
        await _adminService.UpdateEquipmentAsync(id, request, cancellationToken);
        return Ok(await _queries.GetAsync(id, cancellationToken));
    }

    /// <summary>Retires an item that has no current or future bookings; its history is kept.</summary>
    [HttpPost("{id:long}/retirement")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Retire(long id, CancellationToken cancellationToken)
    {
        await _adminService.RetireEquipmentAsync(id, cancellationToken);
        return NoContent();
    }
}
