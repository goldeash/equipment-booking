namespace EquipmentBooking.Api.Common.Cors;

/// <summary>Origins allowed to call the API from a browser, bound from the "Cors" section.</summary>
public sealed class CorsSettings
{
    public const string SectionName = "Cors";
    public const string PolicyName = "spa";

    public string[] AllowedOrigins { get; init; } = [];
}
