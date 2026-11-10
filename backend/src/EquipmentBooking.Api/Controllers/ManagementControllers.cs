using EquipmentBooking.Api.Common.Security;
using EquipmentBooking.Application.Approval;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Loans;
using EquipmentBooking.Application.Maintenance;
using EquipmentBooking.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

/// <summary>Queue of requests routed to the department manager.</summary>
[ApiController]
[Route("api/v1/approval-queue")]
[Produces("application/json")]
[Tags("Approval queue")]
[Authorize(Policy = AuthorizationPolicies.Manager)]
public sealed class ApprovalQueueController : ControllerBase
{
    private readonly ApprovalQueueService _service;

    public ApprovalQueueController(ApprovalQueueService service)
    {
        _service = service;
    }

    /// <summary>Lists the routed requests of the manager's department with the reasons for routing, oldest first.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<ApprovalQueueItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ApprovalQueueItem>>> Get([FromQuery] PageQuery query, CancellationToken cancellationToken) =>
        Ok(await _service.GetQueueAsync(query, cancellationToken));
}

/// <summary>Decision log of the approval route.</summary>
[ApiController]
[Route("api/v1/approval-assessments")]
[Produces("application/json")]
[Tags("Approval assessments")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed class ApprovalAssessmentsController : ControllerBase
{
    private readonly AssessmentLogQueries _queries;

    public ApprovalAssessmentsController(AssessmentLogQueries queries)
    {
        _queries = queries;
    }

    /// <summary>Lists assessments with the result of every policy check, the purpose check and the outcome.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<AssessmentLogEntry>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<AssessmentLogEntry>>> Get([FromQuery] AssessmentLogQuery query, CancellationToken cancellationToken) =>
        Ok(await _queries.GetAsync(query, cancellationToken));
}

/// <summary>Loans for administrators: ready to issue, active and overdue.</summary>
[ApiController]
[Route("api/v1/loans")]
[Produces("application/json")]
[Tags("Loans")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed class LoansController : ControllerBase
{
    private readonly LoanService _service;

    public LoansController(LoanService service)
    {
        _service = service;
    }

    /// <summary>Lists loans by state: "ready" (approved, start date reached), "active" or "overdue".</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<LoanListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<LoanListItem>>> Get([FromQuery] LoansQuery query, CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(query, cancellationToken));
}

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

/// <summary>User accounts, roles and departments.</summary>
[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
[Tags("Users")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed class UsersController : ControllerBase
{
    private readonly UserAdminService _service;

    public UsersController(UserAdminService service)
    {
        _service = service;
    }

    /// <summary>Lists users filtered by name or e-mail, role and department.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<UserResponse>>> Get([FromQuery] UsersQuery query, CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(query, cancellationToken));

    /// <summary>Creates a user with an initial password.</summary>
    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _service.CreateAsync(request, cancellationToken);
        return Created($"/api/v1/users/{user.Id}", user);
    }

    /// <summary>Updates name, role, department and active state; a role change ends the user's sessions.</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UserResponse>> Update(long id, UpdateUserRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAsync(id, request, cancellationToken));

    /// <summary>Sets a new password and ends the user's sessions.</summary>
    [HttpPut("{id:long}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPassword(long id, SetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _service.SetPasswordAsync(id, request, cancellationToken);
        return NoContent();
    }
}
