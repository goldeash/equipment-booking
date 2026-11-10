using System.Reflection;

namespace EquipmentBooking.Infrastructure.Persistence.Sql;

/// <summary>Reads versioned SQL scripts embedded in the assembly for use in migrations.</summary>
internal static class SqlResource
{
    public static string Read(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith($".Persistence.Sql.{fileName}", StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"The SQL resource '{fileName}' is not embedded.");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
