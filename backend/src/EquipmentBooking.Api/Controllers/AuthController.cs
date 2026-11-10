using EquipmentBooking.Api.Common.RateLimiting;
using EquipmentBooking.Api.Common.Security;
using EquipmentBooking.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EquipmentBooking.Api.Controllers;

/// <summary>Sign in, token refresh and sign out.</summary>
[ApiController]
[Route("api/v1/auth/sessions")]
[Produces("application/json")]
[Tags("Authentication")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Signs in with e-mail and password.</summary>
    /// <remarks>Returns an access token for the Authorization header and sets the refresh token as an HttpOnly cookie.</remarks>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Authentication)]
    [ProducesResponseType<SessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SessionResponse>> SignIn(SignInRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.SignInAsync(request, cancellationToken);
        RefreshTokenCookie.Write(Response, result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(result.Session);
    }

    /// <summary>Issues a new access token using the refresh token cookie and rotates the cookie.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Authentication)]
    [ProducesResponseType<SessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SessionResponse>> Refresh(CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(RefreshTokenCookie.Read(Request), cancellationToken);
        RefreshTokenCookie.Write(Response, result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(result.Session);
    }

    /// <summary>Signs out: revokes the refresh token and deletes the cookie.</summary>
    [HttpDelete("current")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SignOut(CancellationToken cancellationToken)
    {
        await _authService.SignOutAsync(RefreshTokenCookie.Read(Request), cancellationToken);
        RefreshTokenCookie.Delete(Response);
        return NoContent();
    }
}
