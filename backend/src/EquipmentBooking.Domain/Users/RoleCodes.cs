namespace EquipmentBooking.Domain.Users;

/// <summary>Codes of the user roles.</summary>
public static class RoleCodes
{
    public const string Employee = "employee";
    public const string Manager = "manager";
    public const string Administrator = "administrator";

    public static readonly IReadOnlyList<string> All = [Employee, Manager, Administrator];
}
