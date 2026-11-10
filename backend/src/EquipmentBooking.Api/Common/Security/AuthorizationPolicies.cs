namespace EquipmentBooking.Api.Common.Security;

/// <summary>Names of the authorisation policies; every endpoint requires a signed in user unless marked anonymous.</summary>
public static class AuthorizationPolicies
{
    public const string Manager = "manager";
    public const string Administrator = "administrator";
}
