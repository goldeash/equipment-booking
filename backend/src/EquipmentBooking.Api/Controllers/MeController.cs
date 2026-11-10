using EquipmentBooking.Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

/// <summary>Profile of the signed in user.</summary>
[ApiController]
[Route("api/v1/me")]
[Produces("application/json")]
[Tags("Current user")]
public sealed class MeController : ControllerBase
{
    private readonly AuthService _authService;

    public MeController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Returns the profile of the signed in user.</summary>
    [HttpGet]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> Get(CancellationToken cancellationToken) =>
        Ok(await _authService.GetCurrentUserAsync(cancellationToken));

    /// <summary>Records that the user has seen the privacy notice (acknowledgement, not consent).</summary>
    [HttpPut("privacy-notice-acknowledgement")]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> AcknowledgePrivacyNotice(CancellationToken cancellationToken) =>
        Ok(await _authService.AcknowledgePrivacyNoticeAsync(cancellationToken));
}
