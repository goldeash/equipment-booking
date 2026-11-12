using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Common;
using Shouldly;

namespace EquipmentBooking.Domain.Tests;

public sealed class ApprovalAssessmentTests
{
    private static readonly PolicyEvaluation AllPassed = Evaluation();

    [Fact]
    public void Failed_checks_route_the_request_without_a_purpose_check()
    {
        var evaluation = Evaluation(failed: [ApprovalCheckTypeCodes.CategoryAutoApproval, ApprovalCheckTypeCodes.ValueThreshold]);

        var assessment = new ApprovalAssessment(1, evaluation, TestData.Now);

        assessment.Outcome.ShouldBe(AssessmentOutcomes.Routed);
        assessment.PurposeResult.ShouldBe(PurposeResults.NotRun);
        assessment.Checks.Where(check => !check.Passed).Select(check => check.CheckTypeCode)
            .ShouldBe([ApprovalCheckTypeCodes.CategoryAutoApproval, ApprovalCheckTypeCodes.ValueThreshold], ignoreOrder: true);
    }

    [Fact]
    public void Checks_keep_the_compared_values()
    {
        var evaluation = new PolicyEvaluation(
        [
            new(ApprovalCheckTypeCodes.OrganisationAutoApproval, true),
            new(ApprovalCheckTypeCodes.CategoryAutoApproval, true),
            new(ApprovalCheckTypeCodes.ValueThreshold, false, 899m, 500m),
            new(ApprovalCheckTypeCodes.NotSpecialised, true),
            new(ApprovalCheckTypeCodes.MaxDuration, true, 3m, 7m),
        ]);

        var assessment = new ApprovalAssessment(1, evaluation, TestData.Now);

        var value = assessment.Checks.Single(check => check.CheckTypeCode == ApprovalCheckTypeCodes.ValueThreshold);
        value.ObservedValue.ShouldBe(899m);
        value.LimitValue.ShouldBe(500m);
        assessment.Checks.Single(check => check.CheckTypeCode == ApprovalCheckTypeCodes.NotSpecialised).ObservedValue.ShouldBeNull();
    }

    [Fact]
    public void Matching_purpose_approves()
    {
        var assessment = new ApprovalAssessment(1, AllPassed, TestData.Now);
        assessment.IsPending.ShouldBeTrue();

        assessment.CompletePurposeCheck(new PurposeAssessment(PurposeResults.Match, "Interview recording", "model", "v1"));

        assessment.Outcome.ShouldBe(AssessmentOutcomes.Approved);
        assessment.ModelName.ShouldBe("model");
    }

    [Theory]
    [InlineData(PurposeResults.NoMatch)]
    [InlineData(PurposeResults.Unavailable)]
    public void Unconfirmed_purpose_routes_to_the_manager(string result)
    {
        var assessment = new ApprovalAssessment(1, AllPassed, TestData.Now);

        assessment.CompletePurposeCheck(new PurposeAssessment(result, null, null, null));

        assessment.Outcome.ShouldBe(AssessmentOutcomes.Routed);
        assessment.PurposeResult.ShouldBe(result);
        Should.Throw<DomainException>(() => assessment.CompletePurposeCheck(PurposeAssessment.Unavailable));
    }

    [Fact]
    public void Purpose_check_must_have_a_final_result()
    {
        var assessment = new ApprovalAssessment(1, AllPassed, TestData.Now);

        Should.Throw<ArgumentException>(() => assessment.CompletePurposeCheck(new PurposeAssessment(PurposeResults.Pending, null, null, null)));
    }

    [Fact]
    public void Evaluation_needs_one_result_for_every_check_type()
    {
        AllPassed.AllPassed.ShouldBeTrue();
        Evaluation(failed: [ApprovalCheckTypeCodes.MaxDuration]).AllPassed.ShouldBeFalse();

        Should.Throw<ArgumentException>(() => new PolicyEvaluation([new(ApprovalCheckTypeCodes.ValueThreshold, true)]));
        Should.Throw<ArgumentException>(() => new PolicyEvaluation(
            ApprovalCheckTypeCodes.All.Select(code => new PolicyCheckResult(code, true)).Append(new(ApprovalCheckTypeCodes.MaxDuration, true))));
    }

    private static PolicyEvaluation Evaluation(params string[] failed) =>
        new(ApprovalCheckTypeCodes.All.Select(code => new PolicyCheckResult(code, !failed.Contains(code))));
}
