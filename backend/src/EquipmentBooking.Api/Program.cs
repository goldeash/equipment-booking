using EquipmentBooking.Api.Common;
using EquipmentBooking.Api.Common.Cors;
using EquipmentBooking.Api.Common.Middleware;
using EquipmentBooking.Api.Common.OpenApi;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new RenderedCompactJsonFormatter()));

    builder.Services.AddApiServices(builder.Configuration);

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseMiddleware<SecurityHeadersMiddleware>();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseCors(CorsSettings.PolicyName);
    app.UseRateLimiter();
    app.UseAuthorization();

    app.MapApiDocumentation();
    app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains(ApiServiceCollectionExtensions.ReadinessTag),
    }).AllowAnonymous();
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "The application failed to start");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

/// <summary>Entry point type, exposed for integration tests.</summary>
public partial class Program;
