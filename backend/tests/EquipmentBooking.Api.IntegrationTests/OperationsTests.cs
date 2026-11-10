using System.Net;
using System.Net.Http.Json;
using EquipmentBooking.Api.IntegrationTests.Infrastructure;
using EquipmentBooking.Application.Approval;
using EquipmentBooking.Application.Bookings;
using EquipmentBooking.Application.Catalogue;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Maintenance;
using EquipmentBooking.Application.Settings;
using EquipmentBooking.Application.Users;
using EquipmentBooking.Domain.Bookings;
using Npgsql;
using Shouldly;

namespace EquipmentBooking.Api.IntegrationTests;

public sealed class OperationsTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public async Task Out_of_service_maintenance_asks_for_confirmation_and_then_cancels_bookings()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var admin = await Factory.SignInAsync(TestData.Administrator);
        var itemId = Data.CheapItemIds[10];
        var booking = await ReadAsync<BookingDetails>(await BookAsync(employee, itemId, Today.AddDays(5), Today.AddDays(6)));
        var request = new CreateMaintenanceRequest("repair", Today.AddDays(4), Today.AddDays(8), true, "Sensor cleaning", false);

        var unconfirmed = await admin.PostAsJsonAsync($"/api/v1/equipment/{itemId}/maintenance-records", request, Cancellation);
        unconfirmed.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await ReadJsonAsync(unconfirmed);
        problem.GetProperty("code").GetString().ShouldBe("maintenance.confirmation_required");
        problem.GetProperty("affectedBookings")[0].GetProperty("bookingId").GetInt64().ShouldBe(booking.Id);

        var confirmed = await admin.PostAsJsonAsync($"/api/v1/equipment/{itemId}/maintenance-records", request with { ConfirmCancellation = true }, Cancellation);
        confirmed.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await ReadAsync<BookingDetails>(await employee.GetAsync($"/api/v1/bookings/{booking.Id}", Cancellation))).Status.ShouldBe("cancelled");

        var blocked = await BookAsync(employee, itemId, Today.AddDays(7), Today.AddDays(7));
        blocked.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ReadJsonAsync(blocked)).GetProperty("code").GetString().ShouldBe("booking.item_under_maintenance");
    }

    [Fact]
    public async Task Retention_job_anonymises_old_returned_loans_and_keeps_item_history()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var manager = await Factory.SignInAsync(TestData.Manager);
        var admin = await Factory.SignInAsync(TestData.Administrator);
        var booking = await ReadAsync<BookingDetails>(await BookAsync(employee, Data.CheapItemIds[11], Today, Today));
        await manager.PostAsJsonAsync($"/api/v1/bookings/{booking.Id}/decisions", new DecideRequest("approved", null), Cancellation);
        await admin.PostAsync($"/api/v1/bookings/{booking.Id}/loan", null, Cancellation);
        await admin.PutAsJsonAsync($"/api/v1/bookings/{booking.Id}/loan/return", new Application.Loans.ReturnLoanRequest("good", null), Cancellation);
        await Factory.ExecuteAsOwnerAsync(
            "UPDATE app.loans SET issued_at = now() - interval '400 days', returned_at = now() - interval '399 days' WHERE booking_id = @id",
            new NpgsqlParameter("id", booking.Id));

        using var trigger = new HttpRequestMessage(HttpMethod.Post, "/internal/jobs/data-retention/runs");
        trigger.Headers.Add("X-Job-Key", ApiFactory.JobKey);
        (await Factory.CreateHttpsClient().SendAsync(trigger, Cancellation)).StatusCode.ShouldBe(HttpStatusCode.OK);

        var requester = await Factory.ScalarAsOwnerAsync<long?>("SELECT requester_id FROM app.bookings WHERE id = @id", new NpgsqlParameter("id", booking.Id));
        var reference = await Factory.ScalarAsOwnerAsync<string>("SELECT anonymous_ref FROM app.bookings WHERE id = @id", new NpgsqlParameter("id", booking.Id));
        var notifications = await Factory.ScalarAsOwnerAsync<long>("SELECT count(*) FROM app.notifications WHERE booking_id = @id", new NpgsqlParameter("id", booking.Id));
        var requesterInHistory = await Factory.ScalarAsOwnerAsync<long>(
            "SELECT count(*) FROM app.booking_status_history WHERE booking_id = @id AND changed_by_user_id = @user",
            new NpgsqlParameter("id", booking.Id),
            new NpgsqlParameter("user", Data.EmployeeId));
        requester.ShouldBeNull();
        reference.ShouldStartWith("anon-");
        notifications.ShouldBe(0);
        requesterInHistory.ShouldBe(0);

        var history = await ReadAsync<List<EquipmentHistoryEvent>>(await admin.GetAsync($"/api/v1/equipment/{Data.CheapItemIds[11]}/history", Cancellation));
        history.ShouldContain(item => item.BookingId == booking.Id && item.Summary.Contains("anonymised"));
    }

    [Fact]
    public async Task Catalogue_search_filters_by_keyword_category_and_availability()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var busyItem = Data.CheapItemIds[12];
        await BookAsync(employee, busyItem, Today.AddDays(30), Today.AddDays(31));

        var byKeyword = await ReadAsync<PagedResult<EquipmentListItem>>(await employee.GetAsync("/api/v1/equipment?q=drone", Cancellation));
        byKeyword.Items.ShouldHaveSingleItem().Id.ShouldBe(Data.SpecialisedItemId);

        var free = await ReadAsync<PagedResult<EquipmentListItem>>(await employee.GetAsync(
            $"/api/v1/equipment?categoryId={Data.CheapCategoryId}&availableFrom={Today.AddDays(30):yyyy-MM-dd}&availableTo={Today.AddDays(30):yyyy-MM-dd}&pageSize=100",
            Cancellation));
        free.Items.ShouldNotContain(item => item.Id == busyItem);
        free.Items.ShouldAllBe(item => item.CategoryId == Data.CheapCategoryId);

        var periods = await ReadAsync<List<BookedPeriod>>(await employee.GetAsync(
            $"/api/v1/equipment/{busyItem}/booked-periods?from={Today:yyyy-MM-dd}&to={Today.AddDays(60):yyyy-MM-dd}", Cancellation));
        periods.ShouldContain(period => period.StartDate == Today.AddDays(30) && period.Kind == "booking");
    }

    [Fact]
    public async Task Invalid_query_returns_validation_problem_details()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);

        var response = await employee.GetAsync("/api/v1/equipment?sort=price", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ReadJsonAsync(response);
        problem.GetProperty("code").GetString().ShouldBe("validation.failed");
        problem.GetProperty("errors").GetProperty("sort").GetArrayLength().ShouldBe(1);
        problem.GetProperty("traceId").GetString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Administrator_manages_equipment_and_duplicate_inventory_numbers_conflict()
    {
        var admin = await Factory.SignInAsync(TestData.Administrator);
        var request = new SaveEquipmentRequest("EQ-TST-500", "Field recorder", "Recorder for interviews", Data.CheapCategoryId, Data.LocationId, 199.90m, "new", false, null);

        var created = await admin.PostAsJsonAsync("/api/v1/equipment", request, Cancellation);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var item = await ReadAsync<EquipmentDetails>(created);
        item.Status.ShouldBe("available");

        var duplicate = await admin.PostAsJsonAsync("/api/v1/equipment", request, Cancellation);
        duplicate.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ReadJsonAsync(duplicate)).GetProperty("code").GetString().ShouldBe("equipment.duplicate_inventory_number");

        (await admin.PostAsync($"/api/v1/equipment/{item.Id}/retirement", null, Cancellation)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var employee = await Factory.SignInAsync(TestData.Employee);
        (await employee.GetAsync($"/api/v1/equipment/{item.Id}", Cancellation)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Item_with_future_bookings_cannot_be_retired()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var admin = await Factory.SignInAsync(TestData.Administrator);
        await BookAsync(employee, Data.CheapItemIds[13], Today.AddDays(40), Today.AddDays(40));

        var response = await admin.PostAsync($"/api/v1/equipment/{Data.CheapItemIds[13]}/retirement", null, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await ReadJsonAsync(response)).GetProperty("code").GetString().ShouldBe("equipment.has_future_bookings");
    }

    [Fact]
    public async Task Switching_off_automatic_approval_is_recorded_as_a_routing_reason()
    {
        var admin = await Factory.SignInAsync(TestData.Administrator);
        var settings = await ReadAsync<ApprovalSettingsResponse>(await admin.GetAsync("/api/v1/settings/approval", Cancellation));
        await admin.PutAsJsonAsync("/api/v1/settings/approval", new UpdateApprovalSettingsRequest(settings.LowValueThreshold, false, settings.RetentionDays, settings.ReturnTime), Cancellation);

        try
        {
            var employee = await Factory.SignInAsync(TestData.Employee);
            var booking = await ReadAsync<BookingDetails>(await BookAsync(employee, Data.CheapItemIds[14], Today.AddDays(15), Today.AddDays(15)));
            booking.Routing!.Reasons.ShouldContain("org_auto_approval_off");

            var log = await ReadAsync<PagedResult<AssessmentLogEntry>>(await admin.GetAsync("/api/v1/approval-assessments?pageSize=100", Cancellation));
            log.Items.ShouldContain(entry => entry.BookingId == booking.Id && !entry.OrganisationAutoApprovalEnabled);
        }
        finally
        {
            await admin.PutAsJsonAsync("/api/v1/settings/approval", new UpdateApprovalSettingsRequest(settings.LowValueThreshold, true, settings.RetentionDays, settings.ReturnTime), Cancellation);
        }
    }

    [Fact]
    public async Task Role_change_ends_the_sessions_of_the_user()
    {
        var admin = await Factory.SignInAsync(TestData.Administrator);
        var created = await ReadAsync<UserResponse>(await admin.PostAsJsonAsync(
            "/api/v1/users",
            new CreateUserRequest("new.person@test.example", "New Person", "employee", 1, ApiFactory.Password),
            Cancellation));
        var user = Factory.CreateHttpsClient();
        await user.PostAsJsonAsync("/api/v1/auth/sessions", new Application.Auth.SignInRequest(created.Email, ApiFactory.Password), Cancellation);

        await admin.PutAsJsonAsync($"/api/v1/users/{created.Id}", new UpdateUserRequest("New Person", "manager", 1, true), Cancellation);

        (await user.PostAsync("/api/v1/auth/sessions/refresh", null, Cancellation)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Exclusion_constraint_uses_the_blocking_statuses_of_the_domain()
    {
        var definition = await Factory.ScalarAsOwnerAsync<string>(
            "SELECT pg_get_constraintdef(oid) FROM pg_constraint WHERE conname = 'ex_bookings_no_overlap'") ?? string.Empty;

        foreach (var status in BookingStatusCodes.Blocking)
        {
            definition.ShouldContain($"'{status}'");
        }

        foreach (var status in BookingStatusCodes.All.Except(BookingStatusCodes.Blocking))
        {
            definition.ShouldNotContain($"'{status}'");
        }
    }

    [Fact]
    public async Task Application_login_cannot_change_the_schema()
    {
        await using var connection = new NpgsqlConnection(Factory.AppConnectionString);
        await connection.OpenAsync(Cancellation);
        await using var command = new NpgsqlCommand("CREATE TABLE app.should_not_exist (id integer)", connection);

        var error = await Should.ThrowAsync<PostgresException>(() => command.ExecuteNonQueryAsync(Cancellation));

        error.SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege);
    }
}
