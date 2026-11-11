namespace EquipmentBooking.Application.Common.Errors;

/// <summary>The requested resource does not exist or is not visible to the user.</summary>
public sealed class NotFoundException : AppException
{
    public NotFoundException(string resource, object key)
        : base(ErrorCodes.NotFound, $"{resource} '{key}' was not found.")
    {
    }
}
