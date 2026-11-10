using EquipmentBooking.DbSeeder;
using EquipmentBooking.Domain.Users;
using EquipmentBooking.Infrastructure.Security;
using Npgsql;

// Loads realistic demo data into the database and optionally exports it as SQL.
//
// Environment variables:
//   ConnectionStrings__Migrations  connection string of the schema owner (required for loading)
//   EB_DEMO_PASSWORD               password set for the three demo accounts (required for loading)
//
// Arguments:
//   --export <file>  writes the SQL script to the file (no password hashes) and does not need a database
//   --no-load        only exports

const string TimeZoneId = "Europe/Vilnius";
const int MinDemoPasswordLength = 12;

var exportPath = ReadOption(args, "--export");
var load = !args.Contains("--no-load");
var now = DateTimeOffset.UtcNow;
var data = new DemoDataGenerator(now, TimeZoneId).Generate();
var script = SqlScriptBuilder.Build(data, now);

if (exportPath is not null)
{
    await File.WriteAllTextAsync(exportPath, script);
    Console.WriteLine($"Exported {data.Bookings.Count} bookings and {data.Items.Count} items to {exportPath}.");
}

if (!load)
{
    return 0;
}

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Migrations");
var demoPassword = Environment.GetEnvironmentVariable("EB_DEMO_PASSWORD");
if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(demoPassword) || demoPassword.Length < MinDemoPasswordLength)
{
    Console.Error.WriteLine(
        $"Set ConnectionStrings__Migrations and EB_DEMO_PASSWORD (at least {MinDemoPasswordLength} characters) to load the data.");
    return 1;
}

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();
await using (var command = new NpgsqlCommand(script, connection))
{
    command.CommandTimeout = 300;
    await command.ExecuteNonQueryAsync();
}

var hashing = new PasswordHashing();
foreach (var email in new[] { DemoDataGenerator.DemoAdministratorEmail, DemoDataGenerator.DemoManagerEmail, DemoDataGenerator.DemoEmployeeEmail })
{
    var row = data.Users.Single(user => user.Email == email);
    var hash = hashing.Hash(new User(row.Email, row.FullName, row.Role, row.DepartmentId), demoPassword);
    await using var update = new NpgsqlCommand("UPDATE app.users SET password_hash = @hash WHERE email = @email", connection);
    update.Parameters.AddWithValue("hash", hash);
    update.Parameters.AddWithValue("email", email);
    await update.ExecuteNonQueryAsync();
}

Console.WriteLine(
    $"Loaded {data.Users.Count} users, {data.Items.Count} items, {data.Bookings.Count} bookings and {data.Maintenance.Count} maintenance records.");
return 0;

static string? ReadOption(string[] arguments, string name)
{
    var index = Array.IndexOf(arguments, name);
    return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1] : null;
}
