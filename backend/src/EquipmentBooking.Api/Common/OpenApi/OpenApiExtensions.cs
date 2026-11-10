using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace EquipmentBooking.Api.Common.OpenApi;

public static class OpenApiExtensions
{
    public const string DocumentName = "v1";

    public static IServiceCollection AddApiOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(DocumentName, options => options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Equipment Booking API",
                Version = "1.0",
                Description = "REST API of the equipment reservation and inventory management system. "
                    + "All requests and responses use JSON; errors follow RFC 9457 problem details with a stable \"code\" field.",
            };
            return Task.CompletedTask;
        }));
        return services;
    }

    /// <summary>Serves the OpenAPI document and the interactive reference at /docs.</summary>
    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi("/openapi/{documentName}.json").AllowAnonymous();
        app.MapScalarApiReference("/docs", options => options
            .WithTitle("Equipment Booking API")
            .WithOpenApiRoutePattern("/openapi/{documentName}.json"))
            .AllowAnonymous();
        return app;
    }
}
