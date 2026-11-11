using EquipmentBooking.Api.Common.Security;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

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
