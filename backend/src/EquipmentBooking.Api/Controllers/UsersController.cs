using EquipmentBooking.Api.Common.Security;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

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
