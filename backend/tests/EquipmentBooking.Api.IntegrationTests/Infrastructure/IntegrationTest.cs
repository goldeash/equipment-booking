using System.Net.Http.Json;
using System.Text.Json;
using EquipmentBooking.Application.Bookings;

namespace EquipmentBooking.Api.IntegrationTests.Infrastructure;

/// <summary>Base class of the database backed API tests.</summary>
[Collection(ApiCollection.Name)]
public abstract class IntegrationTest
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    protected IntegrationTest(ApiFactory factory)
    {
        Factory = factory;
        Assert.SkipUnless(factory.IsConfigured, "Set EB_TEST_DB_OWNER_CONNECTION and EB_TEST_DB_APP_CONNECTION to run the database tests.");
    }

    protected ApiFactory Factory { get; }

    protected TestData Data => Factory.Data;

    protected static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    protected static DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Vilnius")).DateTime);

    protected static async Task<T> ReadAsync<T>(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Json, Cancellation))!;

    protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(Json, Cancellation);

    protected static Task<HttpResponseMessage> BookAsync(HttpClient client, long itemId, DateOnly start, DateOnly end, string purpose = "Recording a customer interview") =>
        client.PostAsJsonAsync("/api/v1/bookings", new CreateBookingRequest(itemId, start, end, purpose), Cancellation);
}
