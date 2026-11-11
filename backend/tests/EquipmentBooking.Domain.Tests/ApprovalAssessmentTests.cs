using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Common;
using Shouldly;

namespace EquipmentBooking.Domain.Tests;

public sealed class ApprovalAssessmentTests
{
    private static readonly PolicyEvaluation AllPassed = new(true, true, true, true, true);

    [Fact]
    public void Failed_checks_route_the_request_with_every_reason()
    {
        var evaluation = new PolicyEvaluation(false, false, true, true, false);

        var assessment = new ApprovalAssessment(1, evaluation, TestData.Now);

        assessment.Outcome.ShouldBe(AssessmentOutcomes.Routed);
        assessment.PurposeResult.ShouldBe(PurposeResults.NotRun);
        assessment.Reasons.Select(reason => reason.RoutingReasonCode).ShouldBe(
            [RoutingReasonCodes.CategoryAutoApprovalOff, RoutingReasonCodes.HighValue, RoutingReasonCodes.Specialised],
            ignoreOrder: true);
    }

    [Fact]
    public void Matching_purpose_approves()
    {
        var assessment = new ApprovalAssessment(1, AllPassed, TestData.Now);
        assessment.IsPending.ShouldBeTrue();

        assessment.CompletePurposeCheck(new PurposeAssessment(PurposeResults.Match, "Interview recording", "model", "v1"));

        assessment.Outcome.ShouldBe(AssessmentOutcomes.Approved);
        assessment.Reasons.ShouldBeEmpty();
        assessment.ModelName.ShouldBe("model");
    }

    [Theory]
    [InlineData(PurposeResults.NoMatch, RoutingReasonCodes.PurposeNotConfirmed)]
    [InlineData(PurposeResults.Unavailable, RoutingReasonCodes.AssistantUnavailable)]
    public void Unconfirmed_purpose_routes_to_the_manager(string result, string reason)
    {
        var assessment = new ApprovalAssessment(1, AllPassed, TestData.Now);

        assessment.CompletePurposeCheck(new PurposeAssessment(result, null, null, null));

        assessment.Outcome.ShouldBe(AssessmentOutcomes.Routed);
        assessment.Reasons.Single().RoutingReasonCode.ShouldBe(reason);
        Should.Throw<DomainException>(() => assessment.CompletePurposeCheck(PurposeAssessment.Unavailable));
    }

    [Fact]
    public void Evaluation_with_all_checks_passed_has_no_reasons()
    {
        AllPassed.AllPassed.ShouldBeTrue();
        new PolicyEvaluation(true, true, false, false, true).FailedReasons
            .ShouldBe([RoutingReasonCodes.OrganisationAutoApprovalOff, RoutingReasonCodes.DurationExceeded]);
    }
}
