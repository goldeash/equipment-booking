using Microsoft.EntityFrameworkCore;

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
            .UseSnakeCaseNamingConvention();
}
