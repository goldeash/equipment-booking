namespace EquipmentBooking.Api.Common.RateLimiting;

/// <summary>Names of the rate limiting policies applied to endpoints.</summary>
public static class RateLimitPolicies
{
    /// <summary>Strict limit for sign in and token refresh, per client IP address.</summary>
    public const string Authentication = "authentication";
}
