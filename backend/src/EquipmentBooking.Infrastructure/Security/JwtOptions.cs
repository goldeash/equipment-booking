using System.ComponentModel.DataAnnotations;

namespace EquipmentBooking.Infrastructure.Security;

/// <summary>Token settings bound from the "Jwt" section; the signing key comes from a secret store.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    /// <summary>HMAC-SHA256 key, at least 32 characters, provided by an environment variable.</summary>
    [Required]
    [MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;

    [Range(1, 60)]
    public int AccessTokenMinutes { get; init; } = 15;

    [Range(1, 30)]
    public int RefreshTokenDays { get; init; } = 7;
}
