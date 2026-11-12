using EquipmentBooking.Domain.Users;
using EquipmentBooking.Infrastructure.Security;
using Npgsql;

namespace EquipmentBooking.Api.IntegrationTests.Infrastructure;

/// <summary>Fixed test data: two departments with managers and employees, an administrator and a small catalogue.</summary>
public sealed class TestData
{
    public const string Administrator = "admin@test.example";
    public const string Manager = "manager@test.example";
    public const string Employee = "employee@test.example";
    public const string Colleague = "colleague@test.example";
    public const string OtherManager = "other.manager@test.example";
    public const string OtherEmployee = "other.employee@test.example";

    public long CheapCategoryId { get; private init; }

    public long SpecialisedCategoryId { get; private init; }

    public long LocationId { get; private init; }

    /// <summary>Low value items; every test uses its own item so tests do not affect each other.</summary>
    public IReadOnlyList<long> CheapItemIds { get; private init; } = [];

    public long ExpensiveItemId { get; private init; }

    public long SpecialisedItemId { get; private init; }

    public long EmployeeId { get; private init; }

    public static async Task<TestData> SeedAsync(string ownerConnectionString, string password)
    {
        await using var connection = new NpgsqlConnection(ownerConnectionString);
        await connection.OpenAsync();
        var hashing = new PasswordHashing();

        async Task<long> InsertAsync(string sql, params object?[] values)
        {
            await using var command = new NpgsqlCommand(sql, connection);
            for (var index = 0; index < values.Length; index++)
            {
                command.Parameters.AddWithValue($"p{index}", values[index] ?? DBNull.Value);
            }

            return (long)(await command.ExecuteScalarAsync())!;
        }

        var engineering = await InsertAsync("INSERT INTO app.departments (name) VALUES (@p0) RETURNING id", "Engineering");
        var marketing = await InsertAsync("INSERT INTO app.departments (name) VALUES (@p0) RETURNING id", "Marketing");

        async Task<long> UserAsync(string email, string name, string role, long departmentId)
        {
            var hash = hashing.Hash(new User(email, name, role, departmentId), password);
            return await InsertAsync(
                "INSERT INTO app.users (email, full_name, password_hash, role_code, department_id) VALUES (@p0, @p1, @p2, @p3, @p4) RETURNING id",
                email, name, hash, role, departmentId);
        }

        await UserAsync(Administrator, "Ada Admin", RoleCodes.Administrator, engineering);
        await UserAsync(Manager, "Max Manager", RoleCodes.Manager, engineering);
        var employeeId = await UserAsync(Employee, "Eve Employee", RoleCodes.Employee, engineering);
        await UserAsync(Colleague, "Carl Colleague", RoleCodes.Employee, engineering);
        await UserAsync(OtherManager, "Mia Marketing", RoleCodes.Manager, marketing);
        await UserAsync(OtherEmployee, "Oscar Other", RoleCodes.Employee, marketing);

        var location = await InsertAsync("INSERT INTO app.locations (name) VALUES (@p0) RETURNING id", "Storage room");
        var cheap = await InsertAsync(
            "INSERT INTO app.categories (name, max_loan_days, usage_guidelines) VALUES (@p0, 7, @p1) RETURNING id",
            "Cameras", "Recording interviews and events for work.");
        var specialised = await InsertAsync(
            "INSERT INTO app.categories (name, max_loan_days, usage_guidelines, is_specialised) VALUES (@p0, 3, @p1, true) RETURNING id",
            "Drones", "Site inspections by certified pilots.");

        var cheapItems = new List<long>();
        for (var index = 1; index <= 30; index++)
        {
            cheapItems.Add(await InsertAsync(
                "INSERT INTO app.equipment_items (inventory_number, name, description, category_id, location_id, value, condition_code) "
                + "VALUES (@p0, @p1, @p2, @p3, @p4, 300, 'good') RETURNING id",
                $"EQ-TST-{index:000}", $"Test camera {index}", "Compact camera for interviews", cheap, location));
        }

        var expensive = await InsertAsync(
            "INSERT INTO app.equipment_items (inventory_number, name, category_id, location_id, value, condition_code) "
            + "VALUES ('EQ-TST-900', 'Cinema camera', @p0, @p1, 4500, 'new') RETURNING id",
            cheap, location);
        var drone = await InsertAsync(
            "INSERT INTO app.equipment_items (inventory_number, name, category_id, location_id, value, condition_code) "
            + "VALUES ('EQ-TST-950', 'Inspection drone', @p0, @p1, 450, 'good') RETURNING id",
            specialised, location);

        return new TestData
        {
            CheapCategoryId = cheap,
            SpecialisedCategoryId = specialised,
            LocationId = location,
            CheapItemIds = cheapItems,
            ExpensiveItemId = expensive,
            SpecialisedItemId = drone,
            EmployeeId = employeeId,
        };
    }
}
