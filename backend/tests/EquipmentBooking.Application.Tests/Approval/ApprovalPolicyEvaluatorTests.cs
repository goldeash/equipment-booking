using EquipmentBooking.Application.Approval;
using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Common;
using EquipmentBooking.Domain.Settings;
using Shouldly;

namespace EquipmentBooking.Application.Tests.Approval;

public sealed class ApprovalPolicyEvaluatorTests
{
    private static readonly DateOnly Start = new(2026, 11, 20);

    private readonly ApprovalPolicyEvaluator _evaluator = new(
    [
        new OrganisationAutoApprovalCheck(),
        new CategoryAutoApprovalCheck(),
        new LowValueCheck(),
        new NotSpecialisedCheck(),
        new WithinMaxDurationCheck(),
    ]);

    [Fact]
    public void Routine_request_passes_every_check()
    {
        var evaluation = _evaluator.Evaluate(Context(value: 120m, days: 3));

        evaluation.AllPassed.ShouldBeTrue();
        evaluation.Checks.Select(check => check.CheckTypeCode).ShouldBe(ApprovalCheckTypeCodes.All, ignoreOrder: true);
    }

    [Fact]
    public void Item_at_the_threshold_is_high_value_and_the_values_are_kept()
    {
        var evaluation = _evaluator.Evaluate(Context(value: 500m, days: 1));

        Failed(evaluation).ShouldBe([ApprovalCheckTypeCodes.ValueThreshold]);
        var check = evaluation.Checks.Single(result => result.CheckTypeCode == ApprovalCheckTypeCodes.ValueThreshold);
        check.ObservedValue.ShouldBe(500m);
        check.LimitValue.ShouldBe(500m);
    }

    [Fact]
    public void Specialised_item_overrides_its_category()
    {
        var evaluation = _evaluator.Evaluate(Context(itemSpecialised: true));

        Failed(evaluation).ShouldBe([ApprovalCheckTypeCodes.NotSpecialised]);
    }

    [Fact]
    public void Loan_longer_than_the_category_limit_is_routed_with_both_lengths()
    {
        var evaluation = _evaluator.Evaluate(Context(days: 8, maxLoanDays: 7));

        Failed(evaluation).ShouldBe([ApprovalCheckTypeCodes.MaxDuration]);
        var check = evaluation.Checks.Single(result => result.CheckTypeCode == ApprovalCheckTypeCodes.MaxDuration);
        check.ObservedValue.ShouldBe(8m);
        check.LimitValue.ShouldBe(7m);
    }

    [Fact]
    public void Every_failed_check_is_reported()
    {
        var evaluation = _evaluator.Evaluate(Context(
            value: 900m,
            days: 10,
            maxLoanDays: 5,
            categorySpecialised: true,
            organisationAutoApproval: false,
            categoryAutoApproval: false));

        Failed(evaluation).ShouldBe(ApprovalCheckTypeCodes.All, ignoreOrder: true);
    }

    internal static ApprovalContext Context(
        decimal value = 100m,
        int days = 2,
        int maxLoanDays = 7,
        bool itemSpecialised = false,
        bool categorySpecialised = false,
        bool organisationAutoApproval = true,
        bool categoryAutoApproval = true)
    {
        var category = new Category("Cameras", null, maxLoanDays, "Interviews and events", categorySpecialised, categoryAutoApproval);
        var item = new EquipmentItem("EQ-CAM-001", "Canon EOS R50", null, 1, 1, value, ItemConditionCodes.Good, itemSpecialised);
        var settings = new OrganisationSettings(500m, organisationAutoApproval, 365, "Europe/Vilnius", new TimeOnly(18, 0));
        return new ApprovalContext(item, category, settings, new DateRange(Start, Start.AddDays(days - 1)));
    }

    private static IReadOnlyList<string> Failed(PolicyEvaluation evaluation) =>
        [.. evaluation.Checks.Where(check => !check.Passed).Select(check => check.CheckTypeCode)];
}
