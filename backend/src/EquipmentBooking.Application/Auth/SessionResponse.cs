namespace EquipmentBooking.Application.Auth;

/// <summary>Access token for the Authorization header and the profile of the signed in user.</summary>
public sealed record SessionResponse(string AccessToken, DateTimeOffset AccessTokenExpiresAt, CurrentUserResponse User);
