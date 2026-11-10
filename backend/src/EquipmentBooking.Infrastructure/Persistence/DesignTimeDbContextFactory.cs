using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBooking.Infrastructure.Persistence;

/// <summary>
/// Creates the context for the dotnet-ef tool. The migrator connection string is read from the
/// environment variable ConnectionStrings__Migrations; adding a migration needs no connection.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string MigrationsConnectionVariable = "ConnectionStrings__Migrations";
    private const string OfflineConnection = "Host=localhost;Database=equipment_booking";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(MigrationsConnectionVariable) ?? OfflineConnection;
        var options = new DbContextOptionsBuilder<AppDbContext>();
        PersistenceOptions.Configure(options, connectionString);
        return new AppDbContext(options.Options);
    }
}
