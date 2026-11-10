using System.Threading.RateLimiting;
using EquipmentBooking.Api.Common.Cors;
using EquipmentBooking.Api.Common.ErrorHandling;
using EquipmentBooking.Api.Common.OpenApi;
using EquipmentBooking.Api.Common.RateLimiting;
using EquipmentBooking.Application.Common.Errors;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Common;

public static class ApiServiceCollectionExtensions
{
    public const string ReadinessTag = "ready";

    /// <summary>Registers controllers, problem details, OpenAPI, CORS, rate limiting and health checks.</summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllers()
            .ConfigureApiBehaviorOptions(options => options.SuppressMapClientErrors = false);

        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance = context.HttpContext.Request.Path;
            context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            context.ProblemDetails.Extensions.TryAdd("code", MapStatusToCode(context.ProblemDetails.Status));
        });
        services.AddExceptionHandler<ApiExceptionHandler>();

        services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = context =>
        {
            var problem = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = "One or more fields are invalid.",
            };
            problem.Extensions["code"] = ErrorCodes.ValidationFailed;
            return new BadRequestObjectResult(problem);
        });

        services.AddApiOpenApi();
        services.AddSpaCors(configuration);
        services.AddApiRateLimiting(configuration);
        services.AddHealthChecks();
        return services;
    }

    private static void AddSpaCors(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();
        services.AddCors(options => options.AddPolicy(CorsSettings.PolicyName, policy => policy
            .WithOrigins(settings.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders(Middleware.CorrelationIdMiddleware.HeaderName)));
    }

    private static void AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() ?? new RateLimitingOptions();
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too many requests",
                    Detail = "The request limit was exceeded. Wait a minute and try again.",
                };
                problem.Extensions["code"] = ErrorCodes.RateLimited;
                await problemDetailsService.WriteAsync(new ProblemDetailsContext { HttpContext = context.HttpContext, ProblemDetails = problem });
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.GlobalPermitsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                    }));

            options.AddPolicy(RateLimitPolicies.Authentication, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.AuthenticationPermitsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                    }));
        });
    }

    private static string MapStatusToCode(int? status) => status switch
    {
        StatusCodes.Status400BadRequest => ErrorCodes.ValidationFailed,
        StatusCodes.Status401Unauthorized => ErrorCodes.Unauthorized,
        StatusCodes.Status403Forbidden => ErrorCodes.Forbidden,
        StatusCodes.Status404NotFound => ErrorCodes.NotFound,
        StatusCodes.Status429TooManyRequests => ErrorCodes.RateLimited,
        _ => ErrorCodes.Unexpected,
    };
}
