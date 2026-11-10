namespace EquipmentBooking.Application.Common.Abstractions;

/// <summary>Signed in user of the current request.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>Id of the signed in user; throws when the request is anonymous.</summary>
    long UserId { get; }

    /// <summary>Id of the signed in user or null for anonymous requests and background jobs.</summary>
    long? UserIdOrNull { get; }

    string Role { get; }

    long DepartmentId { get; }

    bool IsInRole(string role);
}
