using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Users;

namespace EquipmentBooking.Application.Common.Abstractions;

/// <summary>Hashes and verifies passwords with a standard key derivation function.</summary>
public interface IPasswordHashing
{
    string Hash(User user, string password);

    bool Verify(User user, string password);
}

/// <summary>Issues signed access tokens and random refresh tokens.</summary>
public interface ITokenService
{
    AccessToken CreateAccessToken(User user);

    /// <summary>Creates a random refresh token and returns it with its hash for storage.</summary>
    (string Token, string Hash) CreateRefreshToken();

    string HashRefreshToken(string token);

    TimeSpan RefreshTokenLifetime { get; }
}

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>Input of the purpose check: only the purpose text and the category data, never the requester.</summary>
public sealed record PurposeCheckRequest(string Purpose, string CategoryName, string UsageGuidelines, string ItemName);

/// <summary>Checks whether a stated purpose matches the usage guidelines of the category.</summary>
public interface IPurposeAssessor
{
    Task<PurposeAssessment> AssessAsync(PurposeCheckRequest request, CancellationToken cancellationToken);
}

/// <summary>Port for sending notification e-mails; no provider is connected yet.</summary>
public interface IEmailSender
{
    Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken);
}
