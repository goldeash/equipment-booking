using System.Net;
using System.Net.Http.Json;
using EquipmentBooking.Api.IntegrationTests.Infrastructure;
using EquipmentBooking.Application.Auth;
using Shouldly;

namespace EquipmentBooking.Api.IntegrationTests;

public sealed class AuthTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public async Task Sign_in_returns_an_access_token_and_sets_the_refresh_cookie()
    {
        var client = Factory.CreateHttpsClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/sessions", new SignInRequest(TestData.Employee, ApiFactory.Password), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var session = await ReadAsync<SessionResponse>(response);
        session.AccessToken.ShouldNotBeNullOrWhiteSpace();
        session.User.Role.ShouldBe("employee");
        var cookie = response.Headers.GetValues("Set-Cookie").Single();
        cookie.ShouldContain("eb_refresh=");
        cookie.ShouldContain("httponly", Case.Insensitive);
        cookie.ShouldContain("samesite=strict", Case.Insensitive);
        cookie.ShouldContain("path=/api/v1/auth", Case.Insensitive);
    }

    [Theory]
    [InlineData(TestData.Employee, "Wrong-password-123")]
    [InlineData("nobody@test.example", "Wrong-password-123")]
    public async Task Wrong_credentials_give_the_same_generic_error(string email, string password)
    {
        var client = Factory.CreateHttpsClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/sessions", new SignInRequest(email, password), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var problem = await ReadJsonAsync(response);
        problem.GetProperty("code").GetString().ShouldBe("auth.invalid_credentials");
        problem.GetProperty("detail").GetString().ShouldBe("The e-mail address or password is incorrect.");
    }

    [Fact]
    public async Task Refresh_rotates_the_token_and_rejects_reuse_of_the_old_one()
    {
        var client = Factory.CreateHttpsClient();
        var signIn = await client.PostAsJsonAsync("/api/v1/auth/sessions", new SignInRequest(TestData.Colleague, ApiFactory.Password), Cancellation);
        var oldCookie = signIn.Headers.GetValues("Set-Cookie").Single().Split(';')[0];

        var refreshed = await client.PostAsync("/api/v1/auth/sessions/refresh", null, Cancellation);
        refreshed.StatusCode.ShouldBe(HttpStatusCode.OK);

        var attacker = Factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), HandleCookies = false });
        using var reuse = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/sessions/refresh");
        reuse.Headers.Add("Cookie", oldCookie);
        var reused = await attacker.SendAsync(reuse, Cancellation);

        reused.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await client.PostAsync("/api/v1/auth/sessions/refresh", null, Cancellation)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoints_require_a_signed_in_user()
    {
        var response = await Factory.CreateHttpsClient().GetAsync("/api/v1/me", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Employees_cannot_call_administrator_functions()
    {
        var client = await Factory.SignInAsync(TestData.Employee);

        (await client.GetAsync("/api/v1/users", Cancellation)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await client.GetAsync("/api/v1/approval-queue", Cancellation)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Privacy_notice_acknowledgement_is_stored()
    {
        var client = await Factory.SignInAsync(TestData.OtherEmployee);

        var response = await client.PutAsync("/api/v1/me/privacy-notice-acknowledgement", null, Cancellation);

        (await ReadAsync<CurrentUserResponse>(response)).PrivacyNoticeAcknowledged.ShouldBeTrue();
    }

    [Fact]
    public async Task Readiness_check_reaches_the_database()
    {
        var response = await Factory.CreateHttpsClient().GetAsync("/health/ready", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
