namespace EquipmentBooking.Application.Common.Errors;

/// <summary>The user is signed in but may not perform the action.</summary>
public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message)
        : base(ErrorCodes.Forbidden, message)
    {
    }
}
