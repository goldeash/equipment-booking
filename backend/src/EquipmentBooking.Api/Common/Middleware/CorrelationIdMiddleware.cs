using Serilog.Context;

namespace EquipmentBooking.Api.Common.Middleware;

/// <summary>
/// Reads the correlation id from the request header or creates one, returns it in the response
/// and adds it to every log event of the request.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    private const int MaxLength = 64;

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        var incoming = context.Request.Headers[HeaderName].ToString();
        var isValid = incoming.Length is > 0 and <= MaxLength && incoming.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_');
        return isValid ? incoming : Guid.NewGuid().ToString("N");
    }
}
