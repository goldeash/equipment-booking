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

/// <summary>The requested resource does not exist or is not visible to the user.</summary>
public sealed class NotFoundException : AppException
{
    public NotFoundException(string resource, object key)
        : base(ErrorCodes.NotFound, $"{resource} '{key}' was not found.")
    {
    }
}

/// <summary>The user is signed in but may not perform the action.</summary>
public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message)
        : base(ErrorCodes.Forbidden, message)
    {
    }
}

/// <summary>Credentials or tokens are invalid.</summary>
public sealed class AuthenticationException : AppException
{
    public AuthenticationException(string code, string message)
        : base(code, message)
    {
    }
}

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

/// <summary>A business rule checked by the application layer is violated.</summary>
public sealed class BusinessRuleException : AppException
{
    public BusinessRuleException(string code, string message)
        : base(code, message)
    {
    }
}
