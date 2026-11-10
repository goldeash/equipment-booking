using System.Globalization;
using System.Security.Claims;
using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Infrastructure.Security;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EquipmentBooking.Api.Common.Security;

/// <summary>Signed in user taken from the validated access token of the current request.</summary>
public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public long? UserIdOrNull => ReadLong(JwtRegisteredClaimNames.Sub);

    public long UserId => UserIdOrNull ?? throw new AuthenticationException(ErrorCodes.Unauthorized, "Sign in is required.");

    public string Role => Principal?.FindFirstValue(AppClaimTypes.Role) ?? string.Empty;

    public long DepartmentId => ReadLong(AppClaimTypes.DepartmentId) ?? 0;

    public bool IsInRole(string role) => string.Equals(Role, role, StringComparison.Ordinal);

    private long? ReadLong(string claimType) =>
        long.TryParse(Principal?.FindFirstValue(claimType), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
}
