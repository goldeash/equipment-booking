using EquipmentBooking.Application.Approval;
using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Notifications;
using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Shouldly;

namespace EquipmentBooking.Application.Tests.Approval;

public sealed class ApprovalRouterTests
{
    private readonly IAppDbContext _db = Substitute.For<IAppDbContext>();
    private readonly DbSet<ApprovalAssessment> _assessments = Substitute.For<DbSet<ApprovalAssessment>>();
    private readonly DbSet<Notification> _notifications = Substitute.For<DbSet<Notification>>();
    private readonly IPurposeAssessor _purposeAssessor = Substitute.For<IPurposeAssessor>();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 11, 10, 9, 0, 0, TimeSpan.Zero));
    private readonly ApprovalRouter _router;

    public ApprovalRouterTests()
    {
        _db.ApprovalAssessments.Returns(_assessments);
        _db.Notifications.Returns(_notifications);
        var currentUser = Substitute.For<ICurrentUser>();
        var notifications = new NotificationService(_db, currentUser, _time, Options.Create(new NotificationOptions()));
        var evaluator = new ApprovalPolicyEvaluator(
        [
            new OrganisationAutoApprovalCheck(),
            new CategoryAutoApprovalCheck(),
            new LowValueCheck(),
            new NotSpecialisedCheck(),
            new WithinMaxDurationCheck(),
        ]);
        _router = new ApprovalRouter(_db, evaluator, _purposeAssessor, notifications, _time);
    }

    [Fact]
    public async Task Routine_request_with_matching_purpose_is_approved_by_the_assistant()
    {
        var context = ApprovalPolicyEvaluatorTests.Context();
        var booking = NewBooking(context);
        _purposeAssessor.AssessAsync(Arg.Any<PurposeCheckRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PurposeAssessment(PurposeResults.Match, "Interview", "test-model", "v1"));

        var assessment = await _router.RouteAsync(booking, context.Item, context.Category, context.Settings, TestContext.Current.CancellationToken);

        assessment.Outcome.ShouldBe(AssessmentOutcomes.Approved);
        booking.StatusCode.ShouldBe(BookingStatusCodes.Approved);
        booking.CurrentDecision!.DecisionSourceCode.ShouldBe(DecisionSourceCodes.AiAssistant);
        _assessments.Received(1).Add(assessment);
        _notifications.Received(1).Add(Arg.Is<Notification>(notification => notification.NotificationTypeCode == NotificationTypeCodes.RequestApproved));
    }

    [Fact]
    public async Task Purpose_check_receives_no_requester_data()
    {
        var context = ApprovalPolicyEvaluatorTests.Context();
        var booking = NewBooking(context);
        _purposeAssessor.AssessAsync(Arg.Any<PurposeCheckRequest>(), Arg.Any<CancellationToken>()).Returns(PurposeAssessment.Unavailable);

        await _router.RouteAsync(booking, context.Item, context.Category, context.Settings, TestContext.Current.CancellationToken);

        await _purposeAssessor.Received(1).AssessAsync(
            new PurposeCheckRequest("Recording an interview", "Cameras", "Interviews and events", "Canon EOS R50"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Unavailable_assistant_routes_to_the_manager_and_never_declines()
    {
        var context = ApprovalPolicyEvaluatorTests.Context();
        var booking = NewBooking(context);
        _purposeAssessor.AssessAsync(Arg.Any<PurposeCheckRequest>(), Arg.Any<CancellationToken>()).Returns(PurposeAssessment.Unavailable);

        var assessment = await _router.RouteAsync(booking, context.Item, context.Category, context.Settings, TestContext.Current.CancellationToken);

        assessment.Outcome.ShouldBe(AssessmentOutcomes.Routed);
        assessment.PurposeResult.ShouldBe(PurposeResults.Unavailable);
        assessment.Checks.ShouldAllBe(check => check.Passed);
        booking.StatusCode.ShouldBe(BookingStatusCodes.Requested);
        booking.Decisions.ShouldBeEmpty();
    }

    [Fact]
    public async Task High_value_request_is_routed_without_asking_the_assistant()
    {
        var context = ApprovalPolicyEvaluatorTests.Context(value: 1500m);
        var booking = NewBooking(context);

        var assessment = await _router.RouteAsync(booking, context.Item, context.Category, context.Settings, TestContext.Current.CancellationToken);

        assessment.Outcome.ShouldBe(AssessmentOutcomes.Routed);
        assessment.PurposeResult.ShouldBe(PurposeResults.NotRun);
        await _purposeAssessor.DidNotReceiveWithAnyArgs().AssessAsync(default!, TestContext.Current.CancellationToken);
        booking.StatusCode.ShouldBe(BookingStatusCodes.Requested);
    }

    private static Booking NewBooking(ApprovalContext context) =>
        Booking.Request(context.Item, 11, context.Period, "Recording an interview", context.Period.Start);
}
