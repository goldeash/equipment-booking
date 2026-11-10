namespace EquipmentBooking.Domain.Common;

/// <summary>Raised when an operation violates a business rule of the domain.</summary>
public sealed class DomainException : Exception
{
    public DomainException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>Stable machine readable error code, see <see cref="DomainErrorCodes"/>.</summary>
    public string Code { get; }
}
