namespace EquipmentBooking.Api.Common.RateLimiting;

/// <summary>Rate limiting settings bound from the "RateLimiting" configuration section.</summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int GlobalPermitsPerMinute { get; init; } = 300;

    public int AuthenticationPermitsPerMinute { get; init; } = 10;
}
