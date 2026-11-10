using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Users;

/// <summary>Stored hash of a refresh token; tokens are rotated on every use.</summary>
public sealed class RefreshToken : Entity
{
    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    public RefreshToken(long userId, string tokenHash, DateTimeOffset createdAt, DateTimeOffset expiresAt)
    {
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public long UserId { get; private set; }

    public User? User { get; private set; }

    public string TokenHash { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public RefreshToken? ReplacedByToken { get; private set; }

    public long? ReplacedByTokenId { get; private set; }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTimeOffset now) => RevokedAt ??= now;

    public void ReplaceWith(RefreshToken next, DateTimeOffset now)
    {
        Revoke(now);
        ReplacedByToken = next;
    }
}
