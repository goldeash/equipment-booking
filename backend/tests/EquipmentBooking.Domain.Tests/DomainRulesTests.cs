using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Common;
using EquipmentBooking.Domain.Maintenance;
using EquipmentBooking.Domain.Notifications;
using EquipmentBooking.Domain.Settings;
using EquipmentBooking.Domain.Users;
using Shouldly;

namespace EquipmentBooking.Domain.Tests;

public sealed class DateRangeTests
{
    [Fact]
    public void Days_counts_both_ends()
    {
        new DateRange(new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 12)).Days.ShouldBe(3);
    }

    [Fact]
    public void End_before_start_is_rejected()
    {
        Should.Throw<DomainException>(() => new DateRange(new DateOnly(2026, 11, 12), new DateOnly(2026, 11, 10)))
            .Code.ShouldBe(DomainErrorCodes.InvalidDateRange);
    }

    [Theory]
    [InlineData(10, 12, 12, 14, true)]
    [InlineData(10, 12, 13, 14, false)]
    [InlineData(10, 20, 12, 13, true)]
    [InlineData(15, 16, 10, 14, false)]
    public void Overlaps_detects_shared_days(int startA, int endA, int startB, int endB, bool expected)
    {
        var first = new DateRange(new DateOnly(2026, 11, startA), new DateOnly(2026, 11, endA));
        var second = new DateRange(new DateOnly(2026, 11, startB), new DateOnly(2026, 11, endB));

        first.Overlaps(second).ShouldBe(expected);
        first.Contains(new DateOnly(2026, 11, startA)).ShouldBeTrue();
    }
}

public sealed class UserTests
{
    [Fact]
    public void Fifth_failed_sign_in_locks_the_account()
    {
        var user = new User("a@company.example", "Ann Lee", RoleCodes.Employee, 1);
        var now = TestData.Now;

        for (var attempt = 0; attempt < User.MaxFailedSignIns - 1; attempt++)
        {
            user.RegisterFailedSignIn(now);
        }

        user.IsLockedOut(now).ShouldBeFalse();
        user.RegisterFailedSignIn(now);
        user.IsLockedOut(now).ShouldBeTrue();
        user.IsLockedOut(now.Add(User.LockoutDuration)).ShouldBeFalse();
    }

    [Fact]
    public void Successful_sign_in_resets_the_counter()
    {
        var user = new User("a@company.example", "Ann Lee", RoleCodes.Employee, 1);
        user.RegisterFailedSignIn(TestData.Now);

        user.RegisterSuccessfulSignIn();

        user.FailedSignInCount.ShouldBe(0);
        user.LockoutEndsAt.ShouldBeNull();
    }

    [Fact]
    public void Role_change_is_reported_only_when_the_role_differs()
    {
        var user = new User("a@company.example", "Ann Lee", RoleCodes.Employee, 1);

        user.ChangeRole(RoleCodes.Employee).ShouldBeFalse();
        user.ChangeRole(RoleCodes.Manager).ShouldBeTrue();
        user.RoleCode.ShouldBe(RoleCodes.Manager);
    }

    [Fact]
    public void Privacy_notice_acknowledgement_keeps_the_first_time()
    {
        var user = new User("a@company.example", "Ann Lee", RoleCodes.Employee, 1);

        user.AcknowledgePrivacyNotice(TestData.Now);
        user.AcknowledgePrivacyNotice(TestData.Now.AddDays(1));

        user.PrivacyNoticeAcknowledgedAt.ShouldBe(TestData.Now);
    }

    [Fact]
    public void Refresh_token_rotation_revokes_the_old_token()
    {
        var current = new RefreshToken(1, "hash-1", TestData.Now, TestData.Now.AddDays(7));
        var next = new RefreshToken(1, "hash-2", TestData.Now, TestData.Now.AddDays(7));

        current.IsActive(TestData.Now).ShouldBeTrue();
        current.ReplaceWith(next, TestData.Now);

        current.IsActive(TestData.Now).ShouldBeFalse();
        current.ReplacedByToken.ShouldBe(next);
        next.IsActive(TestData.Now.AddDays(8)).ShouldBeFalse();
    }
}

public sealed class CatalogueTests
{
    [Fact]
    public void Category_needs_a_positive_loan_duration()
    {
        Should.Throw<DomainException>(() => new Category("Cameras", null, 0, "Work use", false, true))
            .Code.ShouldBe(DomainErrorCodes.InvalidLoanDuration);
    }

    [Fact]
    public void Category_trims_text_and_drops_empty_description()
    {
        var category = new Category(" Cameras ", " ", 7, " Work use ", true, false);

        category.Name.ShouldBe("Cameras");
        category.Description.ShouldBeNull();
        category.UsageGuidelines.ShouldBe("Work use");
        category.IsSpecialised.ShouldBeTrue();
        category.AutoApprovalEnabled.ShouldBeFalse();
    }

    [Fact]
    public void Item_value_must_not_be_negative()
    {
        Should.Throw<DomainException>(() => TestData.Item(value: -1m)).Code.ShouldBe(DomainErrorCodes.InvalidValue);
    }

    [Fact]
    public void Retired_item_cannot_be_changed()
    {
        var item = TestData.Item();
        item.Retire(TestData.Now);

        item.IsRetired.ShouldBeTrue();
        item.RetiredAt.ShouldBe(TestData.Now);
        Should.Throw<DomainException>(() => item.Update("New", null, 1, 1, 1m, false)).Code.ShouldBe(DomainErrorCodes.ItemRetired);
        Should.Throw<DomainException>(() => item.Retire(TestData.Now)).Code.ShouldBe(DomainErrorCodes.ItemRetired);
    }

    [Fact]
    public void Features_are_replaced()
    {
        var item = TestData.Item();
        item.ReplaceFeatures([new Feature("4K"), new Feature("Wireless")]);
        item.ReplaceFeatures([new Feature("HDMI")]);

        item.Features.Select(feature => feature.Name).ShouldBe(["HDMI"]);
    }
}

public sealed class SettingsTests
{
    [Fact]
    public void Value_equal_to_the_threshold_is_high_value()
    {
        var settings = new OrganisationSettings(500m, true, 365, "Europe/Vilnius", new TimeOnly(18, 0));

        settings.IsLowValue(499.99m).ShouldBeTrue();
        settings.IsLowValue(500m).ShouldBeFalse();
    }

    [Fact]
    public void Threshold_and_retention_are_validated()
    {
        var settings = new OrganisationSettings(500m, true, 365, "Europe/Vilnius", new TimeOnly(18, 0));

        Should.Throw<DomainException>(() => settings.Update(0m, true, 365, new TimeOnly(18, 0), 1)).Code.ShouldBe(DomainErrorCodes.InvalidThreshold);
        Should.Throw<DomainException>(() => settings.Update(100m, true, 29, new TimeOnly(18, 0), 1)).Code.ShouldBe(DomainErrorCodes.InvalidRetentionPeriod);
    }
}

public sealed class MaintenanceTests
{
    [Fact]
    public void Open_ended_maintenance_overlaps_every_later_period()
    {
        var record = new MaintenanceRecord(1, MaintenanceTypeCodes.Repair, TestData.Today, null, true, "Lens", 1);

        record.Overlaps(new DateRange(TestData.Today.AddDays(30), TestData.Today.AddDays(31))).ShouldBeTrue();
        record.Overlaps(new DateRange(TestData.Today.AddDays(-3), TestData.Today.AddDays(-1))).ShouldBeFalse();
    }

    [Fact]
    public void Maintenance_end_must_not_be_before_start()
    {
        Should.Throw<DomainException>(() => new MaintenanceRecord(1, MaintenanceTypeCodes.Repair, TestData.Today, TestData.Today.AddDays(-1), true, "Lens", 1))
            .Code.ShouldBe(DomainErrorCodes.InvalidMaintenancePeriod);
    }
}

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

public sealed class NotificationTests
{
    [Fact]
    public void Email_status_follows_the_email_setting()
    {
        new Notification(1, NotificationTypeCodes.RequestApproved, 5, "Title", "Body", false, TestData.Now).EmailStatus.ShouldBe(EmailStatuses.NotRequired);
        new Notification(1, NotificationTypeCodes.RequestApproved, 5, "Title", "Body", true, TestData.Now).EmailStatus.ShouldBe(EmailStatuses.Pending);
    }

    [Fact]
    public void Read_time_and_delivery_attempts_are_recorded()
    {
        var notification = new Notification(1, NotificationTypeCodes.LoanOverdue, null, "Title", "Body", true, TestData.Now);

        notification.MarkRead(TestData.Now);
        notification.MarkRead(TestData.Now.AddHours(1));
        notification.MarkEmailFailed();
        notification.MarkEmailSent(TestData.Now);

        notification.ReadAt.ShouldBe(TestData.Now);
        notification.EmailAttempts.ShouldBe(2);
        notification.EmailStatus.ShouldBe(EmailStatuses.Sent);
    }
}
