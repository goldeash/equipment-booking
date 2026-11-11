using EquipmentBooking.Api.Common.Security;
using EquipmentBooking.Application.Approval;
using EquipmentBooking.Application.Common.Paging;
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
