using EquipmentBooking.Domain.Users;

namespace EquipmentBooking.Application.Common.Abstractions;

/// <summary>Hashes and verifies passwords with a standard key derivation function.</summary>
public interface IPasswordHashing
{
    string Hash(User user, string password);

    bool Verify(User user, string password);
}
