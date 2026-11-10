using FluentValidation;

namespace EquipmentBooking.Application.Auth;

public sealed record SignInRequest(string Email, string Password);

public sealed class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().MaximumLength(254).EmailAddress();
        RuleFor(request => request.Password).NotEmpty().MaximumLength(200);
    }
}

/// <summary>Profile of the signed in user.</summary>
public sealed record CurrentUserResponse(
    long Id,
    string Email,
    string FullName,
    string Role,
    long DepartmentId,
    string DepartmentName,
    bool PrivacyNoticeAcknowledged);

/// <summary>Access token for the Authorization header and the profile of the signed in user.</summary>
public sealed record SessionResponse(string AccessToken, DateTimeOffset AccessTokenExpiresAt, CurrentUserResponse User);

/// <summary>Result of sign in or refresh: the session for the body and the refresh token for the cookie.</summary>
public sealed record SessionResult(SessionResponse Session, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);
