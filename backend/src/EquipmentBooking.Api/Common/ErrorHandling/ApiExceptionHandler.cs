using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Api.Common.ErrorHandling;

/// <summary>Maps every exception to an RFC 9457 problem details response with a stable error code.</summary>
public sealed partial class ApiExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<ApiExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ValidationException validation => CreateValidationProblem(validation),
            DomainException domain => Create(StatusCodes.Status422UnprocessableEntity, "Business rule violated", domain.Message, domain.Code),
            BusinessRuleException rule => Create(StatusCodes.Status422UnprocessableEntity, "Business rule violated", rule.Message, rule.Code),
            NotFoundException notFound => Create(StatusCodes.Status404NotFound, "Resource not found", notFound.Message, notFound.Code),
            ForbiddenException forbidden => Create(StatusCodes.Status403Forbidden, "Access denied", forbidden.Message, forbidden.Code),
            AuthenticationException authentication => Create(StatusCodes.Status401Unauthorized, "Authentication failed", authentication.Message, authentication.Code),
            ConflictException conflict => CreateConflict(conflict),
            DbUpdateConcurrencyException => Create(
                StatusCodes.Status409Conflict,
                "Concurrent update",
                "The resource was changed by another request. Reload it and try again.",
                ErrorCodes.ConcurrencyConflict),
            BadHttpRequestException badRequest => Create(StatusCodes.Status400BadRequest, "Bad request", badRequest.Message, ErrorCodes.ValidationFailed),
            _ => Create(StatusCodes.Status500InternalServerError, "Unexpected error", "An unexpected error occurred.", ErrorCodes.Unexpected),
        };

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            LogUnexpected(_logger, exception);
        }
        else
        {
            LogHandled(_logger, problem.Status ?? 0, problem.Extensions["code"] as string ?? string.Empty);
        }

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static ProblemDetails Create(int status, string title, string detail, string code)
    {
        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail };
        problem.Extensions["code"] = code;
        return problem;
    }

    private static ProblemDetails CreateValidationProblem(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(error => ToCamelCase(error.PropertyName))
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).Distinct().ToArray());
        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Detail = "One or more fields are invalid.",
        };
        problem.Extensions["code"] = ErrorCodes.ValidationFailed;
        return problem;
    }

    private static ProblemDetails CreateConflict(ConflictException exception)
    {
        var problem = Create(StatusCodes.Status409Conflict, "Conflict", exception.Message, exception.Code);
        foreach (var (key, value) in exception.Details)
        {
            problem.Extensions[key] = value;
        }

        return problem;
    }

    private static string ToCamelCase(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        var parts = propertyName.Split('.');
        return string.Join('.', parts.Select(part => part.Length == 0 ? part : char.ToLowerInvariant(part[0]) + part[1..]));
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception")]
    private static partial void LogUnexpected(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Request failed with status {StatusCode} and code {ErrorCode}")]
    private static partial void LogHandled(ILogger logger, int statusCode, string errorCode);
}
