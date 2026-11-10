namespace EquipmentBooking.Api.Common.Hosting;

/// <summary>
/// Settings used only while the build generates the OpenAPI document. The app is not started
/// for requests in that mode, so these values never reach a database or sign a token.
/// </summary>
internal static class OpenApiGenerationSettings
{
    public static readonly IReadOnlyDictionary<string, string?> Values = new Dictionary<string, string?>
    {
        ["ConnectionStrings:Default"] = "Host=localhost;Database=openapi_generation",
        ["Jwt:Issuer"] = "openapi-generation",
        ["Jwt:Audience"] = "openapi-generation",
        ["Jwt:SigningKey"] = new string('x', 32),
    };
}
