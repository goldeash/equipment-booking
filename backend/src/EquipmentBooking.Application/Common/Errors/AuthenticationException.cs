namespace EquipmentBooking.Application.Common.Errors;

/// <summary>Credentials or tokens are invalid.</summary>
public sealed class AuthenticationException : AppException
{
    public AuthenticationException(string code, string message)
        : base(code, message)
    {
    }
}
