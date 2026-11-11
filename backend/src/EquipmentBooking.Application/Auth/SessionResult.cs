namespace EquipmentBooking.Application.Auth;

/// <summary>Result of sign in or refresh: the session for the body and the refresh token for the cookie.</summary>
public sealed record SessionResult(SessionResponse Session, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);
