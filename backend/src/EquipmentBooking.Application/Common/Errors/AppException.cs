namespace EquipmentBooking.Application.Common.Errors;

/// <summary>Base type of exceptions that the API maps to a problem details response.</summary>
public abstract class AppException : Exception
{
    protected AppException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
