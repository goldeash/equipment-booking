using EquipmentBooking.Domain.Users;

namespace EquipmentBooking.Application.Common.Abstractions;

/// <summary>Issues signed access tokens and random refresh tokens.</summary>
public interface ITokenService
{
    AccessToken CreateAccessToken(User user);

    /// <summary>Creates a random refresh token and returns it with its hash for storage.</summary>
    (string Token, string Hash) CreateRefreshToken();

    string HashRefreshToken(string token);

    TimeSpan RefreshTokenLifetime { get; }
}
