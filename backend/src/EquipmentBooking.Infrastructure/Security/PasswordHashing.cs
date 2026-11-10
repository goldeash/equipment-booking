using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace EquipmentBooking.Infrastructure.Security;

/// <summary>Password hashing with the ASP.NET Core Identity hasher (PBKDF2-HMAC-SHA512, salted, 100 000 iterations).</summary>
public sealed class PasswordHashing : IPasswordHashing
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(User user, string password) => _hasher.HashPassword(user, password);

    public bool Verify(User user, string password) =>
        _hasher.VerifyHashedPassword(user, user.PasswordHash, password) is not PasswordVerificationResult.Failed;
}
