using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Application.Common.Validation;
using EquipmentBooking.Domain.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Auth;

/// <summary>Sign in with e-mail and password, refresh token rotation and sign out (US-35).</summary>
public sealed class AuthService
{
    private const string InvalidCredentialsMessage = "The e-mail address or password is incorrect.";

    private readonly IAppDbContext _db;
    private readonly IPasswordHashing _passwordHashing;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly IValidator<SignInRequest> _validator;

    public AuthService(
        IAppDbContext db,
        IPasswordHashing passwordHashing,
        ITokenService tokenService,
        ICurrentUser currentUser,
        TimeProvider timeProvider,
        IValidator<SignInRequest> validator)
    {
        _db = db;
        _passwordHashing = passwordHashing;
        _tokenService = tokenService;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _validator = validator;
    }

    public async Task<SessionResult> SignInAsync(SignInRequest request, CancellationToken cancellationToken)
    {
        await _validator.EnsureValidAsync(request, cancellationToken);
        var now = _timeProvider.GetUtcNow();
        var email = request.Email.Trim();
        var user = await _db.Users.Include(candidate => candidate.Department)
            .SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);

        // The same error is returned for unknown accounts, wrong passwords, lockout and inactive
        // accounts, so the response never reveals which part was wrong or whether the account exists.
        if (user is null || !user.IsActive || user.IsLockedOut(now))
        {
            throw new AuthenticationException(ErrorCodes.InvalidCredentials, InvalidCredentialsMessage);
        }

        if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHashing.Verify(user, request.Password))
        {
            user.RegisterFailedSignIn(now);
            await _db.SaveChangesAsync(cancellationToken);
            throw new AuthenticationException(ErrorCodes.InvalidCredentials, InvalidCredentialsMessage);
        }

        user.RegisterSuccessfulSignIn();
        var result = IssueSession(user, now, previous: null);
        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<SessionResult> RefreshAsync(string? refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw InvalidRefreshToken();
        }

        var now = _timeProvider.GetUtcNow();
        var hash = _tokenService.HashRefreshToken(refreshToken);
        var stored = await _db.RefreshTokens
            .Include(token => token.User!).ThenInclude(user => user.Department)
            .SingleOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);
        if (stored is null)
        {
            throw InvalidRefreshToken();
        }

        if (stored.RevokedAt is not null)
        {
            // A revoked token was presented again: treat it as stolen and end every session of the user.
            await RevokeAllAsync(stored.UserId, now, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            throw InvalidRefreshToken();
        }

        var user = stored.User!;
        if (!stored.IsActive(now) || !user.IsActive)
        {
            throw InvalidRefreshToken();
        }

        var result = IssueSession(user, now, stored);
        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task SignOutAsync(string? refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var hash = _tokenService.HashRefreshToken(refreshToken);
        var stored = await _db.RefreshTokens.SingleOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);
        if (stored is not null)
        {
            stored.Revoke(_timeProvider.GetUtcNow());
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var user = await LoadCurrentUserAsync(cancellationToken);
        return ToResponse(user);
    }

    public async Task<CurrentUserResponse> AcknowledgePrivacyNoticeAsync(CancellationToken cancellationToken)
    {
        var user = await LoadCurrentUserAsync(cancellationToken);
        user.AcknowledgePrivacyNotice(_timeProvider.GetUtcNow());
        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(user);
    }

    /// <summary>Revokes every active refresh token of a user, for example after a role change.</summary>
    public async Task RevokeAllAsync(long userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var tokens = await _db.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var token in tokens)
        {
            token.Revoke(now);
        }
    }

    internal static CurrentUserResponse ToResponse(User user) => new(
        user.Id,
        user.Email,
        user.FullName,
        user.RoleCode,
        user.DepartmentId,
        user.Department?.Name ?? string.Empty,
        user.PrivacyNoticeAcknowledgedAt is not null);

    private async Task<User> LoadCurrentUserAsync(CancellationToken cancellationToken) =>
        await _db.Users.Include(user => user.Department)
            .SingleOrDefaultAsync(user => user.Id == _currentUser.UserId, cancellationToken)
        ?? throw new AuthenticationException(ErrorCodes.Unauthorized, "The session is no longer valid.");

    private SessionResult IssueSession(User user, DateTimeOffset now, RefreshToken? previous)
    {
        var accessToken = _tokenService.CreateAccessToken(user);
        var (token, hash) = _tokenService.CreateRefreshToken();
        var expiresAt = now.Add(_tokenService.RefreshTokenLifetime);
        var refresh = new RefreshToken(user.Id, hash, now, expiresAt);
        _db.RefreshTokens.Add(refresh);
        previous?.ReplaceWith(refresh, now);

        var session = new SessionResponse(accessToken.Token, accessToken.ExpiresAt, ToResponse(user));
        return new SessionResult(session, token, expiresAt);
    }

    private static AuthenticationException InvalidRefreshToken() =>
        new(ErrorCodes.InvalidRefreshToken, "The session has expired. Please sign in again.");
}
