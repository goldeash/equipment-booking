using EquipmentBooking.Api.Common.Security;
using EquipmentBooking.Application.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

/// <summary>Organisation settings of the approval route and data retention.</summary>
[ApiController]
[Route("api/v1/settings/approval")]
[Produces("application/json")]
[Tags("Settings")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed class SettingsController : ControllerBase
{
    private readonly SettingsService _service;

    public SettingsController(SettingsService service)
    {
        _service = service;
    }

    /// <summary>Returns the low value threshold, the automatic approval switch, the retention period and the return time.</summary>
    [HttpGet]
    [ProducesResponseType<ApprovalSettingsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApprovalSettingsResponse>> Get(CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(cancellationToken));

    /// <summary>Updates the settings; they apply to the next processed request.</summary>
    [HttpPut]
    [ProducesResponseType<ApprovalSettingsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApprovalSettingsResponse>> Update(UpdateApprovalSettingsRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAsync(request, cancellationToken));
}
