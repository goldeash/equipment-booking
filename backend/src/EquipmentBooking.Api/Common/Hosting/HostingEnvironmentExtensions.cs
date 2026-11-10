using System.Reflection;

namespace EquipmentBooking.Api.Common.Hosting;

public static class HostingEnvironmentExtensions
{
    private const string OpenApiGeneratorAssembly = "GetDocument.Insider";

    /// <summary>
    /// True while the build generates the OpenAPI document. The app is started without a database,
    /// secrets or background jobs in that mode.
    /// </summary>
    public static bool IsOpenApiGeneration() =>
        Assembly.GetEntryAssembly()?.GetName().Name == OpenApiGeneratorAssembly;
}
