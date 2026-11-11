using System.Net.Http.Headers;
using System.Net.Http.Json;
using EquipmentBooking.Application.Auth;
using EquipmentBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EquipmentBooking.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Runs the API against a real PostgreSQL test database. The schema is rebuilt with the migrations
/// before the tests, using the owner connection; the API itself connects with the application login.
/// Connection strings come from the environment variables EB_TEST_DB_OWNER_CONNECTION and
/// EB_TEST_DB_APP_CONNECTION; without them the database tests are skipped.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string JobKey = "integration-test-job-key";
    public const string Password = "Integration-Test-Password-1";

    private const string OwnerVariable = "EB_TEST_DB_OWNER_CONNECTION";
    private const string AppVariable = "EB_TEST_DB_APP_CONNECTION";

    public string? OwnerConnectionString { get; } = Environment.GetEnvironmentVariable(OwnerVariable);

    public string? AppConnectionString { get; } = Environment.GetEnvironmentVariable(AppVariable);

    public bool IsConfigured => !string.IsNullOrWhiteSpace(OwnerConnectionString) && !string.IsNullOrWhiteSpace(AppConnectionString);

    public TestData Data { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        if (!IsConfigured)
        {
            return;
        }

        await using (var connection = new NpgsqlConnection(OwnerConnectionString))
        {
            await connection.OpenAsync();
            await using var reset = new NpgsqlCommand("DROP SCHEMA IF EXISTS app CASCADE;", connection);
            await reset.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(OwnerConnectionString, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", AppDbContext.Schema))
            .UseSnakeCaseNamingConvention()
            .Options;
        await using (var context = new AppDbContext(options))
        {
            await context.Database.MigrateAsync();
        }

        Data = await TestData.SeedAsync(OwnerConnectionString!, Password);
    }

    public new async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
    }

    /// <summary>Creates a client that is signed in as the given user.</summary>
    public async Task<HttpClient> SignInAsync(string email)
    {
        var client = CreateHttpsClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/sessions", new SignInRequest(email, Password));
        response.EnsureSuccessStatusCode();
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session!.AccessToken);
        return client;
    }

    public HttpClient CreateHttpsClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), HandleCookies = true });

    public async Task ExecuteAsOwnerAsync(string sql, params NpgsqlParameter[] parameters)
    {
        await using var connection = new NpgsqlConnection(OwnerConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<T?> ScalarAsOwnerAsync<T>(string sql, params NpgsqlParameter[] parameters)
    {
        await using var connection = new NpgsqlConnection(OwnerConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        var result = await command.ExecuteScalarAsync();
        return result is null or DBNull ? default : (T)result;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = AppConnectionString ?? "Host=localhost;Database=not_configured",
            ["Jwt:SigningKey"] = "integration-tests-signing-key-with-enough-length",
            ["Jobs:SchedulerEnabled"] = "false",
            ["Jobs:TriggerKey"] = JobKey,
            ["RateLimiting:GlobalPermitsPerMinute"] = "100000",
            ["RateLimiting:AuthenticationPermitsPerMinute"] = "100000",
        }));
    }
}
