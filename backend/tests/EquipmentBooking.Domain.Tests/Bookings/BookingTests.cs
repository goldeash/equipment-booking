using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Common;
using Shouldly;

namespace EquipmentBooking.Domain.Tests.Bookings;

public sealed class BookingTests
{
    [Fact]
    public void Request_creates_a_blocking_requested_booking()
    {
        var booking = TestData.RequestedBooking();

        booking.StatusCode.ShouldBe(BookingStatusCodes.Requested);
        booking.IsBlocking.ShouldBeTrue();
        booking.Purpose.ShouldBe("Recording an interview");
        booking.Decisions.ShouldBeEmpty();
    }

    [Fact]
    public void Request_with_start_in_the_past_is_rejected()
    {
        var period = new DateRange(TestData.Today.AddDays(-1), TestData.Today);

        var error = Should.Throw<DomainException>(() => Booking.Request(TestData.Item(), 1, period, "Interview", TestData.Today));

        error.Code.ShouldBe(DomainErrorCodes.StartDateInPast);
    }

    [Theory]
    [InlineData("  ")]
    [InlineData("ab")]
    public void Request_with_too_short_purpose_is_rejected(string purpose)
    {
        var period = new DateRange(TestData.Today, TestData.Today);

        Should.Throw<DomainException>(() => Booking.Request(TestData.Item(), 1, period, purpose, TestData.Today))
            .Code.ShouldBe(DomainErrorCodes.InvalidPurpose);
    }

    [Fact]
    public void Request_for_a_retired_item_is_rejected()
    {
        var item = TestData.Item();
        item.Retire(TestData.Now);

        Should.Throw<DomainException>(() => Booking.Request(item, 1, new DateRange(TestData.Today, TestData.Today), "Interview", TestData.Today))
            .Code.ShouldBe(DomainErrorCodes.ItemRetired);
    }

    [Fact]
    public void Manager_approval_records_a_current_decision()
    {
        var booking = TestData.RequestedBooking();

        booking.ApproveByManager(2, "  Enjoy  ", TestData.Now);

        booking.StatusCode.ShouldBe(BookingStatusCodes.Approved);
        booking.CurrentDecision.ShouldNotBeNull();
        booking.CurrentDecision.DecisionSourceCode.ShouldBe(DecisionSourceCodes.Manager);
        booking.CurrentDecision.DecidedByUserId.ShouldBe(2);
        booking.CurrentDecision.Comment.ShouldBe("Enjoy");
    }

    [Fact]
    public void Assistant_approval_has_no_deciding_user()
    {
        var booking = TestData.RequestedBooking();

        booking.ApproveByAssistant(TestData.Now);

        booking.CurrentDecision!.DecisionSourceCode.ShouldBe(DecisionSourceCodes.AiAssistant);
        booking.CurrentDecision.DecidedByUserId.ShouldBeNull();
        booking.CurrentDecision.Outcome.ShouldBe(DecisionOutcomes.Approved);
    }

    [Fact]
    public void Decline_requires_a_comment()
    {
        var booking = TestData.RequestedBooking();

        Should.Throw<DomainException>(() => booking.Decline(2, " ", TestData.Now)).Code.ShouldBe(DomainErrorCodes.DeclineCommentRequired);
        booking.StatusCode.ShouldBe(BookingStatusCodes.Requested);
    }

    [Fact]
    public void Decline_frees_the_dates()
    {
        var booking = TestData.RequestedBooking();

        booking.Decline(2, "Use the meeting room kit.", TestData.Now);

        booking.StatusCode.ShouldBe(BookingStatusCodes.Declined);
        booking.IsBlocking.ShouldBeFalse();
    }

    [Fact]
    public void Approving_twice_is_an_invalid_transition()
    {
        var booking = TestData.ApprovedBooking();

        Should.Throw<DomainException>(() => booking.ApproveByManager(2, null, TestData.Now)).Code.ShouldBe(DomainErrorCodes.InvalidTransition);
    }

    [Fact]
    public void Changing_dates_supersedes_the_decision_and_requests_again()
    {
        var booking = TestData.ApprovedBooking(startInDays: 2);
        var newPeriod = new DateRange(TestData.Today.AddDays(5), TestData.Today.AddDays(6));

        booking.ChangeDates(newPeriod, TestData.Today, TestData.Now);

        booking.StatusCode.ShouldBe(BookingStatusCodes.Requested);
        booking.Period.ShouldBe(newPeriod);
        booking.CurrentDecision.ShouldBeNull();
        booking.Decisions.Single().SupersededAt.ShouldBe(TestData.Now);
    }

    [Fact]
    public void Cancel_is_possible_before_issue_only()
    {
        var approved = TestData.ApprovedBooking();
        approved.Cancel();
        approved.StatusCode.ShouldBe(BookingStatusCodes.Cancelled);

        var issued = TestData.IssuedBooking();
        Should.Throw<DomainException>(issued.Cancel).Code.ShouldBe(DomainErrorCodes.InvalidTransition);
    }

    [Fact]
    public void Request_expires_after_its_start_date_without_a_decision()
    {
        var booking = TestData.RequestedBooking(startInDays: 1);
        var startDate = TestData.Today.AddDays(1);

        booking.IsExpired(startDate).ShouldBeFalse();
        booking.IsExpired(startDate.AddDays(1)).ShouldBeTrue();

        booking.Expire(startDate.AddDays(1));
        booking.StatusCode.ShouldBe(BookingStatusCodes.Cancelled);
        booking.IsBlocking.ShouldBeFalse();
    }

    [Fact]
    public void Approved_booking_expires_after_its_end_date_when_not_collected()
    {
        var booking = TestData.ApprovedBooking(startInDays: 0);
        var endDate = TestData.Today.AddDays(2);

        booking.IsExpired(endDate).ShouldBeFalse();
        booking.IsExpired(endDate.AddDays(1)).ShouldBeTrue();

        booking.Expire(endDate.AddDays(1));
        booking.StatusCode.ShouldBe(BookingStatusCodes.Cancelled);
    }

    [Fact]
    public void Booking_that_has_not_expired_cannot_be_expired()
    {
        var requested = TestData.RequestedBooking(startInDays: 1);
        Should.Throw<DomainException>(() => requested.Expire(TestData.Today.AddDays(1))).Code.ShouldBe(DomainErrorCodes.InvalidTransition);

        var issued = TestData.IssuedBooking();
        issued.IsExpired(TestData.Today.AddDays(30)).ShouldBeFalse();
        Should.Throw<DomainException>(() => issued.Expire(TestData.Today.AddDays(30))).Code.ShouldBe(DomainErrorCodes.InvalidTransition);
    }

    [Fact]
    public void Issue_before_the_start_date_is_rejected()
    {
        var booking = TestData.ApprovedBooking(startInDays: 3);

        Should.Throw<DomainException>(() => booking.Issue(1, TestData.Today, TestData.Now.AddDays(4), TestData.Now))
            .Code.ShouldBe(DomainErrorCodes.StartDateNotReached);
    }

    [Fact]
    public void Issue_of_a_requested_booking_is_not_available()
    {
        var booking = TestData.RequestedBooking(startInDays: 0);

        Should.Throw<DomainException>(() => booking.Issue(1, TestData.Today, TestData.Now, TestData.Now))
            .Code.ShouldBe(DomainErrorCodes.InvalidTransition);
    }

    [Fact]
    public void Issue_creates_a_loan_with_the_due_time()
    {
        var booking = TestData.IssuedBooking();

        booking.StatusCode.ShouldBe(BookingStatusCodes.Issued);
        booking.Loan.ShouldNotBeNull();
        booking.Loan.DueAt.ShouldBe(TestData.Now.AddDays(2));
        booking.Loan.IssuedByUserId.ShouldBe(1);
    }

    [Fact]
    public void Overdue_loan_keeps_blocking_and_can_be_returned()
    {
        var booking = TestData.IssuedBooking();

        booking.MarkOverdue();
        booking.IsBlocking.ShouldBeTrue();
        booking.Return(1, ItemConditionCodes.Good, null, TestData.Now.AddDays(3));

        booking.StatusCode.ShouldBe(BookingStatusCodes.Returned);
        booking.Loan!.ReturnedAt.ShouldBe(TestData.Now.AddDays(3));
        booking.IsBlocking.ShouldBeFalse();
    }

    [Fact]
    public void Damaged_return_requires_a_comment()
    {
        var booking = TestData.IssuedBooking();

        Should.Throw<DomainException>(() => booking.Return(1, ItemConditionCodes.Damaged, null, TestData.Now))
            .Code.ShouldBe(DomainErrorCodes.DamageCommentRequired);

        booking.Return(1, ItemConditionCodes.Damaged, " Cracked screen ", TestData.Now);
        booking.Loan!.ReturnComment.ShouldBe("Cracked screen");
    }

    [Fact]
    public void Only_issued_loans_can_become_overdue()
    {
        Should.Throw<DomainException>(TestData.ApprovedBooking().MarkOverdue).Code.ShouldBe(DomainErrorCodes.InvalidTransition);
    }

    [Fact]
    public void Anonymise_replaces_the_requester_of_a_returned_loan()
    {
        var booking = TestData.IssuedBooking();
        booking.Return(1, ItemConditionCodes.Good, null, TestData.Now);

        booking.Anonymise("anon-1", TestData.Now);

        booking.RequesterId.ShouldBeNull();
        booking.AnonymousRef.ShouldBe("anon-1");
        booking.AnonymisedAt.ShouldBe(TestData.Now);
    }

    [Fact]
    public void Active_bookings_cannot_be_anonymised()
    {
        Should.Throw<DomainException>(() => TestData.IssuedBooking().Anonymise("anon-1", TestData.Now))
            .Code.ShouldBe(DomainErrorCodes.InvalidTransition);
    }

    [Fact]
    public void Blocking_statuses_match_the_booking_rule()
    {
        BookingStatusCodes.Blocking.ShouldBe(
            [BookingStatusCodes.Requested, BookingStatusCodes.Approved, BookingStatusCodes.Issued, BookingStatusCodes.Overdue],
            ignoreOrder: true);
    }
}
