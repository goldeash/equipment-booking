namespace EquipmentBooking.Application.Common.Errors;

/// <summary>The request conflicts with the current state of a resource.</summary>
public sealed class ConflictException : AppException
{
    public ConflictException(string code, string message, IReadOnlyDictionary<string, object?>? details = null)
        : base(code, message)
    {
        Details = details ?? new Dictionary<string, object?>();
    }

    /// <summary>Additional data for the client, for example the conflicting dates.</summary>
    public IReadOnlyDictionary<string, object?> Details { get; }
}
