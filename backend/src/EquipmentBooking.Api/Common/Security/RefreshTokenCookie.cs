namespace EquipmentBooking.Api.Common.Security;

/// <summary>HttpOnly cookie that carries the refresh token; it is sent only to the authentication endpoints.</summary>
public static class RefreshTokenCookie
{
    public const string Name = "eb_refresh";
    public const string Path = "/api/v1/auth";

    public static void Write(HttpResponse response, string token, DateTimeOffset expiresAt) =>
        response.Cookies.Append(Name, token, CreateOptions(expiresAt));

    public static string? Read(HttpRequest request) => request.Cookies[Name];

    public static void Delete(HttpResponse response) =>
        response.Cookies.Delete(Name, CreateOptions(null));

    private static CookieOptions CreateOptions(DateTimeOffset? expiresAt) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = Path,
        Expires = expiresAt,
        IsEssential = true,
    };
}
