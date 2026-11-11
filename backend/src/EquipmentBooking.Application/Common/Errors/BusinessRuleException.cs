namespace EquipmentBooking.Application.Common.Errors;

/// <summary>A business rule checked by the application layer is violated.</summary>
public sealed class BusinessRuleException : AppException
{
    public BusinessRuleException(string code, string message)
        : base(code, message)
    {
    }
}
