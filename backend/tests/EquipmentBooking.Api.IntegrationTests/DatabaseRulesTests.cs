using System.Net;
using System.Net.Http.Json;
using EquipmentBooking.Api.IntegrationTests.Infrastructure;
using EquipmentBooking.Application.Approval;
using EquipmentBooking.Application.Bookings;
using EquipmentBooking.Application.Catalogue;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Loans;
using EquipmentBooking.Application.Settings;
using EquipmentBooking.Domain.Bookings;
using Npgsql;
using Shouldly;

namespace EquipmentBooking.Api.IntegrationTests;

/// <summary>Business rules that the database enforces on its own, also for writes that bypass the API.</summary>
public sealed class DatabaseRulesTests(ApiFactory factory) : IntegrationTest(factory)
{
    private const string AdministratorIdSql = "(SELECT id FROM app.users WHERE email = 'admin@test.example')";

    [Fact]
    public async Task Transition_table_matches_the_domain_model()
    {
        var stored = await Factory.ScalarAsOwnerAsync<string>(
            "SELECT string_agg(from_status_code || '>' || to_status_code, ',' ORDER BY from_status_code, to_status_code) FROM app.booking_status_transitions");

        var expected = string.Join(',', BookingStatusCodes.Transitions
            .OrderBy(transition => transition.From, StringComparer.Ordinal)
            .ThenBy(transition => transition.To, StringComparer.Ordinal)
            .Select(transition => $"{transition.From}>{transition.To}"));
        stored.ShouldBe(expected);
    }

    [Fact]
    public async Task Status_change_outside_the_transitions_is_rejected()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var booking = await ReadAsync<BookingDetails>(await BookAsync(employee, Data.CheapItemIds[20], Today.AddDays(40), Today.AddDays(40)));

        var error = await Should.ThrowAsync<PostgresException>(() => Factory.ExecuteAsOwnerAsync(
            "UPDATE app.bookings SET status_code = 'returned' WHERE id = @id", new NpgsqlParameter("id", booking.Id)));

        error.SqlState.ShouldBe("EB002");
    }

    [Fact]
    public async Task Approved_booking_without_a_current_approval_is_rejected_at_commit()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var manager = await Factory.SignInAsync(TestData.Manager);
        var booking = await ReadAsync<BookingDetails>(await BookAsync(employee, Data.CheapItemIds[21], Today.AddDays(40), Today.AddDays(40)));
        await manager.PostAsJsonAsync($"/api/v1/bookings/{booking.Id}/decisions", new DecideRequest("approved", null), Cancellation);

        var error = await Should.ThrowAsync<PostgresException>(() => Factory.ExecuteAsOwnerAsync(
            "UPDATE app.booking_decisions SET superseded_at = now() WHERE booking_id = @id", new NpgsqlParameter("id", booking.Id)));

        error.SqlState.ShouldBe("EB005");
    }

    [Fact]
    public async Task Assessment_outcome_must_follow_from_its_checks()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var booking = await ReadAsync<BookingDetails>(await BookAsync(employee, Data.CheapItemIds[22], Today.AddDays(40), Today.AddDays(40)));

        var checks = await Factory.ScalarAsOwnerAsync<long>(
            "SELECT count(*) FROM app.approval_checks c JOIN app.approval_assessments a ON a.id = c.assessment_id WHERE a.booking_id = @id",
            new NpgsqlParameter("id", booking.Id));
        checks.ShouldBe(5);

        var error = await Should.ThrowAsync<PostgresException>(() => Factory.ExecuteAsOwnerAsync(
            "UPDATE app.approval_assessments SET outcome = 'approved' WHERE booking_id = @id", new NpgsqlParameter("id", booking.Id)));

        error.SqlState.ShouldBe("EB006");
    }

    [Fact]
    public async Task Overdue_loan_blocks_the_item_until_it_is_returned()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var manager = await Factory.SignInAsync(TestData.Manager);
        var admin = await Factory.SignInAsync(TestData.Administrator);
        var itemId = Data.CheapItemIds[23];
        var loan = await ReadAsync<BookingDetails>(await BookAsync(employee, itemId, Today, Today));
        await manager.PostAsJsonAsync($"/api/v1/bookings/{loan.Id}/decisions", new DecideRequest("approved", null), Cancellation);
        await admin.PostAsync($"/api/v1/bookings/{loan.Id}/loan", null, Cancellation);
        await Factory.ExecuteAsOwnerAsync("UPDATE app.loans SET due_at = now() - interval '1 hour' WHERE booking_id = @id", new NpgsqlParameter("id", loan.Id));
        await RunJobAsync("overdue-loans");

        var later = await BookAsync(employee, itemId, Today.AddDays(10), Today.AddDays(11));
        later.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await ReadJsonAsync(later)).GetProperty("code").GetString().ShouldBe("booking.item_overdue");

        var free = await ReadAsync<PagedResult<EquipmentListItem>>(await employee.GetAsync(
            $"/api/v1/equipment?availableFrom={Today.AddDays(10):yyyy-MM-dd}&availableTo={Today.AddDays(11):yyyy-MM-dd}&pageSize=100", Cancellation));
        free.Items.ShouldNotContain(item => item.Id == itemId);
        var periods = await ReadAsync<List<BookedPeriod>>(await employee.GetAsync(
            $"/api/v1/equipment/{itemId}/booked-periods?from={Today:yyyy-MM-dd}&to={Today.AddDays(30):yyyy-MM-dd}", Cancellation));
        periods.ShouldContain(period => period.Kind == "overdue" && period.EndDate == null);

        var direct = await Should.ThrowAsync<PostgresException>(() => Factory.ExecuteAsOwnerAsync(
            "INSERT INTO app.bookings (equipment_item_id, requester_id, start_date, end_date, purpose, status_code) VALUES (@item, @user, @start, @start, 'Direct insert', 'requested')",
            new NpgsqlParameter("item", itemId),
            new NpgsqlParameter("user", Data.EmployeeId),
            new NpgsqlParameter("start", Today.AddDays(20))));
        direct.SqlState.ShouldBe("EB003");

        (await admin.PutAsJsonAsync($"/api/v1/bookings/{loan.Id}/loan/return", new ReturnLoanRequest("good", null), Cancellation)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await BookAsync(employee, itemId, Today.AddDays(10), Today.AddDays(11))).StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Out_of_service_maintenance_cannot_overlap_a_blocking_booking()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var itemId = Data.CheapItemIds[24];
        await BookAsync(employee, itemId, Today.AddDays(40), Today.AddDays(41));

        var error = await Should.ThrowAsync<PostgresException>(() => Factory.ExecuteAsOwnerAsync(
            $"INSERT INTO app.maintenance_records (equipment_item_id, maintenance_type_code, start_date, takes_out_of_service, description, recorded_by_user_id) VALUES (@item, 'repair', @start, true, 'Direct insert', {AdministratorIdSql})",
            new NpgsqlParameter("item", itemId),
            new NpgsqlParameter("start", Today.AddDays(41))));

        error.SqlState.ShouldBe("EB001");
    }

    [Fact]
    public async Task Actions_need_the_right_role()
    {
        var error = await Should.ThrowAsync<PostgresException>(() => Factory.ExecuteAsOwnerAsync(
            "INSERT INTO app.maintenance_records (equipment_item_id, maintenance_type_code, start_date, takes_out_of_service, description, recorded_by_user_id) VALUES (@item, 'inspection', @start, false, 'Direct insert', @user)",
            new NpgsqlParameter("item", Data.CheapItemIds[25]),
            new NpgsqlParameter("start", Today.AddDays(50)),
            new NpgsqlParameter("user", Data.EmployeeId)));

        error.SqlState.ShouldBe("EB004");
    }

    [Fact]
    public async Task Manager_cannot_decide_an_own_request()
    {
        var manager = await Factory.SignInAsync(TestData.Manager);
        var booking = await ReadAsync<BookingDetails>(await BookAsync(manager, Data.CheapItemIds[26], Today.AddDays(42), Today.AddDays(42)));

        var queue = await ReadAsync<PagedResult<ApprovalQueueItem>>(await manager.GetAsync("/api/v1/approval-queue?pageSize=100", Cancellation));
        queue.Items.ShouldNotContain(item => item.BookingId == booking.Id);

        var decision = await manager.PostAsJsonAsync($"/api/v1/bookings/{booking.Id}/decisions", new DecideRequest("approved", null), Cancellation);
        decision.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await ReadJsonAsync(decision)).GetProperty("code").GetString().ShouldBe("booking.own_request");

        var direct = await Should.ThrowAsync<PostgresException>(() => Factory.ExecuteAsOwnerAsync(
            "INSERT INTO app.booking_decisions (booking_id, decision_source_code, decided_by_user_id, outcome, decided_at) SELECT id, 'manager', requester_id, 'approved', now() FROM app.bookings WHERE id = @id",
            new NpgsqlParameter("id", booking.Id)));
        direct.SqlState.ShouldBe("EB004");
    }

    [Fact]
    public async Task Booking_with_history_cannot_be_deleted()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var booking = await ReadAsync<BookingDetails>(await BookAsync(employee, Data.CheapItemIds[27], Today.AddDays(43), Today.AddDays(43)));

        var error = await Should.ThrowAsync<PostgresException>(() => Factory.ExecuteAsOwnerAsync(
            "DELETE FROM app.bookings WHERE id = @id", new NpgsqlParameter("id", booking.Id)));

        error.SqlState.ShouldBe(PostgresErrorCodes.RestrictViolation);
    }

    [Fact]
    public async Task Every_settings_change_is_kept_in_the_history()
    {
        var admin = await Factory.SignInAsync(TestData.Administrator);
        var settings = await ReadAsync<ApprovalSettingsResponse>(await admin.GetAsync("/api/v1/settings/approval", Cancellation));
        var before = await Factory.ScalarAsOwnerAsync<long>("SELECT count(*) FROM app.organisation_settings_history");

        await admin.PutAsJsonAsync("/api/v1/settings/approval", new UpdateApprovalSettingsRequest(settings.LowValueThreshold + 1, settings.AutoApprovalEnabled, settings.RetentionDays, settings.ReturnTime), Cancellation);
        try
        {
            (await Factory.ScalarAsOwnerAsync<long>("SELECT count(*) FROM app.organisation_settings_history")).ShouldBe(before + 1);
            var latest = await Factory.ScalarAsOwnerAsync<decimal>("SELECT low_value_threshold FROM app.organisation_settings_history ORDER BY id DESC LIMIT 1");
            latest.ShouldBe(settings.LowValueThreshold + 1);
            var author = await Factory.ScalarAsOwnerAsync<string>(
                "SELECT u.email::text FROM app.organisation_settings_history h JOIN app.users u ON u.id = h.changed_by_user_id ORDER BY h.id DESC LIMIT 1");
            author.ShouldBe(TestData.Administrator);
        }
        finally
        {
            await admin.PutAsJsonAsync("/api/v1/settings/approval", new UpdateApprovalSettingsRequest(settings.LowValueThreshold, settings.AutoApprovalEnabled, settings.RetentionDays, settings.ReturnTime), Cancellation);
        }
    }

    private async Task RunJobAsync(string name)
    {
        using var trigger = new HttpRequestMessage(HttpMethod.Post, $"/internal/jobs/{name}/runs");
        trigger.Headers.Add("X-Job-Key", ApiFactory.JobKey);
        (await Factory.CreateHttpsClient().SendAsync(trigger, Cancellation)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
