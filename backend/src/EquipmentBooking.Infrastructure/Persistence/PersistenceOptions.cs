using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace EquipmentBooking.Infrastructure.Persistence;

internal static class PersistenceOptions
{
    public const string MigrationsHistoryTable = "__EFMigrationsHistory";

    /// <summary>Shared provider settings for the application and the design-time factory.</summary>
    public static void Configure(DbContextOptionsBuilder options, string connectionString) =>
        options
            .UseNpgsql(connectionString, npgsql => npgsql
                .MigrationsHistoryTable(MigrationsHistoryTable, AppDbContext.Schema)
                .MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention()
            // Constraint violations are translated into conflict responses and logged by the API, so the
            // provider level error events are lowered to debug to avoid duplicate error entries.
            .ConfigureWarnings(warnings => warnings.Log(
                (RelationalEventId.CommandError, LogLevel.Debug),
                (CoreEventId.SaveChangesFailed, LogLevel.Debug)));
}
