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
    }

    [Fact]
    public void Item_at_the_threshold_is_high_value()
    {
        var evaluation = _evaluator.Evaluate(Context(value: 500m, days: 1));

        evaluation.ValueCheckPassed.ShouldBeFalse();
        evaluation.FailedReasons.ShouldBe([RoutingReasonCodes.HighValue]);
    }

    [Fact]
    public void Specialised_item_overrides_its_category()
    {
        var evaluation = _evaluator.Evaluate(Context(itemSpecialised: true));

        evaluation.FailedReasons.ShouldBe([RoutingReasonCodes.Specialised]);
    }

    [Fact]
    public void Loan_longer_than_the_category_limit_is_routed()
    {
        var evaluation = _evaluator.Evaluate(Context(days: 8, maxLoanDays: 7));

        evaluation.FailedReasons.ShouldBe([RoutingReasonCodes.DurationExceeded]);
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

        evaluation.FailedReasons.ShouldBe(
        [
            RoutingReasonCodes.OrganisationAutoApprovalOff,
            RoutingReasonCodes.CategoryAutoApprovalOff,
            RoutingReasonCodes.HighValue,
            RoutingReasonCodes.Specialised,
            RoutingReasonCodes.DurationExceeded,
        ]);
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
}
