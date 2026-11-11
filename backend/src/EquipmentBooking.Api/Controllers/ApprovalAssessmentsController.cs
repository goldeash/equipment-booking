using EquipmentBooking.Api.Common.Security;
using EquipmentBooking.Application.Approval;
using EquipmentBooking.Application.Common.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

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
