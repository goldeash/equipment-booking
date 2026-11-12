using System.Net;
using System.Net.Http.Json;
using EquipmentBooking.Api.IntegrationTests.Infrastructure;
using EquipmentBooking.Application.Approval;
using EquipmentBooking.Application.Bookings;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Loans;
using EquipmentBooking.Application.Notifications;
using Npgsql;
using Shouldly;

namespace EquipmentBooking.Api.IntegrationTests;

public sealed class BookingWorkflowTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public async Task Routine_request_is_routed_to_the_manager_while_no_assistant_is_configured()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);

        var response = await BookAsync(employee, Data.CheapItemIds[0], Today.AddDays(5), Today.AddDays(6));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var booking = await ReadAsync<BookingDetails>(response);
        booking.Status.ShouldBe("requested");
        booking.Routing!.Outcome.ShouldBe("routed");
        booking.Routing.Reasons.ShouldBe(["assistant_unavailable"]);
        booking.CanCancel.ShouldBeTrue();
    }

    [Fact]
    public async Task High_value_and_specialised_items_are_routed_with_their_reasons()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);

        var expensive = await ReadAsync<BookingDetails>(await BookAsync(employee, Data.ExpensiveItemId, Today.AddDays(10), Today.AddDays(10)));
        var drone = await ReadAsync<BookingDetails>(await BookAsync(employee, Data.SpecialisedItemId, Today.AddDays(10), Today.AddDays(14)));

        expensive.Routing!.Reasons.ShouldBe(["high_value"]);
        drone.Routing!.Reasons.ShouldBe(["specialised", "duration_exceeded"], ignoreOrder: true);
        drone.Routing.PurposeResult.ShouldBe("not_run");
    }

    [Fact]
    public async Task Overlapping_request_is_rejected_with_the_conflicting_dates()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var colleague = await Factory.SignInAsync(TestData.Colleague);
        var itemId = Data.CheapItemIds[1];
        (await BookAsync(employee, itemId, Today.AddDays(10), Today.AddDays(12))).StatusCode.ShouldBe(HttpStatusCode.Created);

        var response = await BookAsync(colleague, itemId, Today.AddDays(11), Today.AddDays(13));

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await ReadJsonAsync(response);
        problem.GetProperty("code").GetString().ShouldBe("booking.overlap");
        var period = problem.GetProperty("conflictingPeriods")[0];
        period.GetProperty("startDate").GetString().ShouldBe(Today.AddDays(10).ToString("yyyy-MM-dd"));
        period.GetProperty("endDate").GetString().ShouldBe(Today.AddDays(12).ToString("yyyy-MM-dd"));
    }

    [Fact]
    public async Task Simultaneous_overlapping_requests_accept_exactly_one()
    {
        var clients = await Task.WhenAll(Enumerable.Range(0, 8).Select(index => Factory.SignInAsync(index % 2 == 0 ? TestData.Employee : TestData.Colleague)));
        var itemId = Data.CheapItemIds[2];

        var responses = await Task.WhenAll(clients.Select(client => BookAsync(client, itemId, Today.AddDays(20), Today.AddDays(21))));

        responses.Count(response => response.StatusCode == HttpStatusCode.Created).ShouldBe(1);
        responses.Count(response => response.StatusCode == HttpStatusCode.Conflict).ShouldBe(7);
    }

    [Fact]
    public async Task Request_starting_in_the_past_is_rejected()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);

        var response = await BookAsync(employee, Data.CheapItemIds[3], Today.AddDays(-1), Today);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await ReadJsonAsync(response)).GetProperty("code").GetString().ShouldBe("booking.start_date_in_past");
    }

    [Fact]
    public async Task Employee_can_change_dates_and_cancel_own_booking()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var booking = await ReadAsync<BookingDetails>(await BookAsync(employee, Data.CheapItemIds[4], Today.AddDays(3), Today.AddDays(4)));

        var changed = await employee.PatchAsJsonAsync($"/api/v1/bookings/{booking.Id}", new ChangeBookingDatesRequest(Today.AddDays(8), Today.AddDays(9)), Cancellation);
        changed.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadAsync<BookingDetails>(changed)).StartDate.ShouldBe(Today.AddDays(8));

        var cancelled = await employee.PostAsync($"/api/v1/bookings/{booking.Id}/cancellation", null, Cancellation);
        var cancelledDetails = await ReadAsync<BookingDetails>(cancelled);
        cancelledDetails.Status.ShouldBe("cancelled");
        cancelledDetails.Cancellation!.Code.ShouldBe("by_requester");

        (await BookAsync(employee, Data.CheapItemIds[4], Today.AddDays(8), Today.AddDays(9))).StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Bookings_are_hidden_from_other_employees_and_other_departments()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var booking = await ReadAsync<BookingDetails>(await BookAsync(employee, Data.CheapItemIds[5], Today.AddDays(3), Today.AddDays(3)));

        var colleague = await Factory.SignInAsync(TestData.Colleague);
        var otherManager = await Factory.SignInAsync(TestData.OtherManager);
        var manager = await Factory.SignInAsync(TestData.Manager);

        (await colleague.GetAsync($"/api/v1/bookings/{booking.Id}", Cancellation)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await otherManager.GetAsync($"/api/v1/bookings/{booking.Id}", Cancellation)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await manager.GetAsync($"/api/v1/bookings/{booking.Id}", Cancellation)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Approval_queue_pages_do_not_overlap()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        foreach (var itemId in Data.CheapItemIds.Skip(15).Take(3))
        {
            (await BookAsync(employee, itemId, Today.AddDays(20), Today.AddDays(21))).StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        var manager = await Factory.SignInAsync(TestData.Manager);
        var all = await ReadAsync<PagedResult<ApprovalQueueItem>>(await manager.GetAsync("/api/v1/approval-queue?pageSize=100", Cancellation));
        all.TotalCount.ShouldBeGreaterThanOrEqualTo(3);

        var paged = new List<long>();
        for (var page = 1; page <= all.TotalCount; page++)
        {
            var result = await ReadAsync<PagedResult<ApprovalQueueItem>>(await manager.GetAsync($"/api/v1/approval-queue?page={page}&pageSize=1", Cancellation));
            result.Page.ShouldBe(page);
            paged.AddRange(result.Items.Select(item => item.BookingId));
        }

        paged.ShouldBe(all.Items.Select(item => item.BookingId).ToList());
    }

    [Fact]
    public async Task Manager_decides_requests_of_the_own_department_only()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var booking = await ReadAsync<BookingDetails>(await BookAsync(employee, Data.CheapItemIds[6], Today.AddDays(4), Today.AddDays(5)));
        var manager = await Factory.SignInAsync(TestData.Manager);
        var otherManager = await Factory.SignInAsync(TestData.OtherManager);

        var queue = await ReadAsync<PagedResult<ApprovalQueueItem>>(await manager.GetAsync("/api/v1/approval-queue?pageSize=100", Cancellation));
        queue.Items.ShouldContain(item => item.BookingId == booking.Id && item.Reasons.Any(reason => reason.Code == "assistant_unavailable"));
        var otherQueue = await ReadAsync<PagedResult<ApprovalQueueItem>>(await otherManager.GetAsync("/api/v1/approval-queue?pageSize=100", Cancellation));
        otherQueue.Items.ShouldNotContain(item => item.BookingId == booking.Id);

        (await otherManager.PostAsJsonAsync($"/api/v1/bookings/{booking.Id}/decisions", new DecideRequest("approved", null), Cancellation))
            .StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await manager.PostAsJsonAsync($"/api/v1/bookings/{booking.Id}/decisions", new DecideRequest("declined", null), Cancellation))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var declined = await manager.PostAsJsonAsync($"/api/v1/bookings/{booking.Id}/decisions", new DecideRequest("declined", "Use the room camera."), Cancellation);
        var details = await ReadAsync<BookingDetails>(declined);
        details.Status.ShouldBe("declined");
        details.Decision!.Source.ShouldBe("manager");
        details.Decision.DecidedByName.ShouldBe("Max Manager");

        var notifications = await ReadAsync<PagedResult<NotificationResponse>>(await employee.GetAsync("/api/v1/me/notifications?pageSize=100", Cancellation));
        notifications.Items.ShouldContain(notification => notification.BookingId == booking.Id && notification.Type == "request_declined");
    }

    [Fact]
    public async Task Manager_sees_the_return_history_of_the_requester()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var booking = await ReadAsync<BookingDetails>(await BookAsync(employee, Data.CheapItemIds[7], Today.AddDays(6), Today.AddDays(6)));
        var manager = await Factory.SignInAsync(TestData.Manager);

        var history = await ReadAsync<RequesterHistoryResponse>(await manager.GetAsync($"/api/v1/bookings/{booking.Id}/requester-history", Cancellation));

        history.RequesterName.ShouldBe("Eve Employee");
        history.LoanCount.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Approved_booking_is_issued_and_returned_with_the_condition()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var manager = await Factory.SignInAsync(TestData.Manager);
        var admin = await Factory.SignInAsync(TestData.Administrator);
        var itemId = Data.CheapItemIds[8];
        var booking = await ReadAsync<BookingDetails>(await BookAsync(employee, itemId, Today, Today.AddDays(1)));
        await manager.PostAsJsonAsync($"/api/v1/bookings/{booking.Id}/decisions", new DecideRequest("approved", null), Cancellation);

        var ready = await ReadAsync<PagedResult<LoanListItem>>(await admin.GetAsync("/api/v1/loans?state=ready&pageSize=100", Cancellation));
        ready.Items.ShouldContain(loan => loan.BookingId == booking.Id);

        var issued = await ReadAsync<BookingDetails>(await admin.PostAsync($"/api/v1/bookings/{booking.Id}/loan", null, Cancellation));
        issued.Status.ShouldBe("issued");
        issued.Loan!.DueAt.ShouldBeGreaterThan(issued.Loan.IssuedAt);

        var damagedWithoutComment = await admin.PutAsJsonAsync($"/api/v1/bookings/{booking.Id}/loan/return", new ReturnLoanRequest("damaged", null), Cancellation);
        damagedWithoutComment.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var returned = await ReadAsync<BookingDetails>(await admin.PutAsJsonAsync($"/api/v1/bookings/{booking.Id}/loan/return", new ReturnLoanRequest("damaged", "Cracked lens."), Cancellation));
        returned.Status.ShouldBe("returned");
        returned.Loan!.ReturnConditionCode.ShouldBe("damaged");

        var condition = await Factory.ScalarAsOwnerAsync<string>("SELECT condition_code FROM app.equipment_items WHERE id = @id", new NpgsqlParameter("id", itemId));
        condition.ShouldBe("damaged");
        var history = await Factory.ScalarAsOwnerAsync<long>(
            "SELECT count(*) FROM app.booking_status_history WHERE booking_id = @id", new NpgsqlParameter("id", booking.Id));
        history.ShouldBe(4);
    }

    [Fact]
    public async Task Overdue_job_flags_loans_after_the_due_time_and_notifies()
    {
        var employee = await Factory.SignInAsync(TestData.Employee);
        var manager = await Factory.SignInAsync(TestData.Manager);
        var admin = await Factory.SignInAsync(TestData.Administrator);
        var booking = await ReadAsync<BookingDetails>(await BookAsync(employee, Data.CheapItemIds[9], Today, Today));
        await manager.PostAsJsonAsync($"/api/v1/bookings/{booking.Id}/decisions", new DecideRequest("approved", null), Cancellation);
        await admin.PostAsync($"/api/v1/bookings/{booking.Id}/loan", null, Cancellation);
        await Factory.ExecuteAsOwnerAsync("UPDATE app.loans SET due_at = now() - interval '2 hours' WHERE booking_id = @id", new NpgsqlParameter("id", booking.Id));

        using var trigger = new HttpRequestMessage(HttpMethod.Post, "/internal/jobs/overdue-loans/runs");
        trigger.Headers.Add("X-Job-Key", ApiFactory.JobKey);
        (await Factory.CreateHttpsClient().SendAsync(trigger, Cancellation)).StatusCode.ShouldBe(HttpStatusCode.OK);

        (await ReadAsync<BookingDetails>(await employee.GetAsync($"/api/v1/bookings/{booking.Id}", Cancellation))).Status.ShouldBe("overdue");
        var notifications = await ReadAsync<PagedResult<NotificationResponse>>(await admin.GetAsync("/api/v1/me/notifications?pageSize=100", Cancellation));
        notifications.Items.ShouldContain(notification => notification.BookingId == booking.Id && notification.Type == "loan_overdue");
    }

    [Fact]
    public async Task Job_trigger_requires_the_secret_key()
    {
        using var trigger = new HttpRequestMessage(HttpMethod.Post, "/internal/jobs/overdue-loans/runs");
        trigger.Headers.Add("X-Job-Key", "wrong");

        (await Factory.CreateHttpsClient().SendAsync(trigger, Cancellation)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
