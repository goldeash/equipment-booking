namespace EquipmentBooking.Api.Common.RateLimiting;

/// <summary>Names of the rate limiting policies applied to endpoints.</summary>
public static class RateLimitPolicies
{
    /// <summary>Strict limit for sign in and token refresh, per client IP address.</summary>
    public const string Authentication = "authentication";
}

/// <summary>Rate limiting settings bound from the "RateLimiting" configuration section.</summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int GlobalPermitsPerMinute { get; init; } = 300;

    public int AuthenticationPermitsPerMinute { get; init; } = 10;
}
