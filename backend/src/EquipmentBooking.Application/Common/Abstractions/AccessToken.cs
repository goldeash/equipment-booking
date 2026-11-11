namespace EquipmentBooking.Application.Common.Abstractions;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);
