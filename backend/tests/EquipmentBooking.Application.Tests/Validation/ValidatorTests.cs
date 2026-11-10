using EquipmentBooking.Application.Approval;
using EquipmentBooking.Application.Auth;
using EquipmentBooking.Application.Bookings;
using EquipmentBooking.Application.Catalogue;
using EquipmentBooking.Application.Loans;
using EquipmentBooking.Application.Maintenance;
using EquipmentBooking.Application.Settings;
using EquipmentBooking.Application.Users;
using FluentValidation.TestHelper;
using Shouldly;

namespace EquipmentBooking.Application.Tests.Validation;

public sealed class ValidatorTests
{
    private static readonly DateOnly Day = new(2026, 11, 20);

    [Fact]
    public void Booking_request_needs_ordered_dates_and_a_purpose()
    {
        var validator = new CreateBookingRequestValidator();

        validator.TestValidate(new CreateBookingRequest(1, Day, Day.AddDays(2), "Client meeting")).ShouldNotHaveAnyValidationErrors();
        var result = validator.TestValidate(new CreateBookingRequest(0, Day, Day.AddDays(-1), "ab"));
        result.ShouldHaveValidationErrorFor(request => request.EquipmentItemId);
        result.ShouldHaveValidationErrorFor(request => request.EndDate);
        result.ShouldHaveValidationErrorFor(request => request.Purpose);
    }

    [Fact]
    public void Date_change_needs_ordered_dates()
    {
        new ChangeBookingDatesRequestValidator().TestValidate(new ChangeBookingDatesRequest(Day, Day.AddDays(-1)))
            .ShouldHaveValidationErrorFor(request => request.EndDate);
    }

    [Fact]
    public void Decline_needs_a_comment_and_outcome_must_be_known()
    {
        var validator = new DecideRequestValidator();

        validator.TestValidate(new DecideRequest("approved", null)).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new DecideRequest("declined", " ")).ShouldHaveValidationErrorFor(request => request.Comment);
        validator.TestValidate(new DecideRequest("maybe", null)).ShouldHaveValidationErrorFor(request => request.Outcome);
    }

    [Fact]
    public void Damaged_return_needs_a_comment()
    {
        var validator = new ReturnLoanRequestValidator();

        validator.TestValidate(new ReturnLoanRequest("good", null)).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new ReturnLoanRequest("damaged", null)).ShouldHaveValidationErrorFor(request => request.Comment);
        validator.TestValidate(new ReturnLoanRequest("broken", null)).ShouldHaveValidationErrorFor(request => request.ConditionCode);
    }

    [Fact]
    public void Equipment_requires_valid_inventory_number_and_condition()
    {
        var validator = new SaveEquipmentRequestValidator();
        var valid = new SaveEquipmentRequest("EQ-CAM-099", "Canon EOS R50", null, 1, 1, 799.99m, "good", false, [1, 2]);

        validator.TestValidate(valid).ShouldNotHaveAnyValidationErrors();
        var result = validator.TestValidate(valid with { InventoryNumber = "EQ CAM", Value = -1m, ConditionCode = "unknown" });
        result.ShouldHaveValidationErrorFor(request => request.InventoryNumber);
        result.ShouldHaveValidationErrorFor(request => request.Value);
        result.ShouldHaveValidationErrorFor(request => request.ConditionCode);
    }

    [Fact]
    public void Category_requires_guidelines_and_positive_loan_limit()
    {
        var result = new SaveCategoryRequestValidator().TestValidate(new SaveCategoryRequest("Cameras", null, 0, "", false, true));

        result.ShouldHaveValidationErrorFor(request => request.MaxLoanDays);
        result.ShouldHaveValidationErrorFor(request => request.UsageGuidelines);
    }

    [Fact]
    public void Catalogue_query_validates_sort_and_availability_range()
    {
        var validator = new EquipmentListQueryValidator();

        validator.TestValidate(new EquipmentListQuery { Sort = "-value", AvailableFrom = Day, AvailableTo = Day }).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new EquipmentListQuery { Sort = "price" }).ShouldHaveValidationErrorFor(query => query.Sort);
        validator.TestValidate(new EquipmentListQuery { AvailableFrom = Day }).IsValid.ShouldBeFalse();
        validator.TestValidate(new EquipmentListQuery { AvailableFrom = Day, AvailableTo = Day.AddDays(-1) })
            .ShouldHaveValidationErrorFor(query => query.AvailableTo);
    }

    [Fact]
    public void Booked_periods_range_is_limited()
    {
        var validator = new BookedPeriodsQueryValidator();

        validator.TestValidate(new BookedPeriodsQuery(Day, Day.AddDays(60))).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new BookedPeriodsQuery(Day, Day.AddDays(400))).IsValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData("short1", false)]
    [InlineData("onlyletterspassword", false)]
    [InlineData("Long-enough-password-1", true)]
    public void User_password_must_be_long_and_mixed(string password, bool valid)
    {
        var result = new CreateUserRequestValidator().TestValidate(new CreateUserRequest("ann.lee@company.example", "Ann Lee", "employee", 1, password));

        result.IsValid.ShouldBe(valid);
    }

    [Fact]
    public void User_role_must_be_known()
    {
        new UpdateUserRequestValidator().TestValidate(new UpdateUserRequest("Ann Lee", "owner", 1, true))
            .ShouldHaveValidationErrorFor(request => request.Role);
    }

    [Fact]
    public void Sign_in_needs_an_email_and_a_password()
    {
        var result = new SignInRequestValidator().TestValidate(new SignInRequest("not-an-email", ""));

        result.ShouldHaveValidationErrorFor(request => request.Email);
        result.ShouldHaveValidationErrorFor(request => request.Password);
    }

    [Fact]
    public void Settings_need_positive_threshold_and_minimum_retention()
    {
        var result = new UpdateApprovalSettingsRequestValidator().TestValidate(new UpdateApprovalSettingsRequest(0m, true, 10, new TimeOnly(18, 0)));

        result.ShouldHaveValidationErrorFor(request => request.LowValueThreshold);
        result.ShouldHaveValidationErrorFor(request => request.RetentionDays);
    }

    [Fact]
    public void Maintenance_needs_a_description_and_ordered_dates()
    {
        var result = new CreateMaintenanceRequestValidator().TestValidate(new CreateMaintenanceRequest("repair", Day, Day.AddDays(-1), true, "", false));

        result.ShouldHaveValidationErrorFor(request => request.EndDate);
        result.ShouldHaveValidationErrorFor(request => request.Description);
    }

    [Fact]
    public void List_queries_validate_their_filters()
    {
        new MyBookingsQueryValidator().TestValidate(new MyBookingsQuery { Status = "lost" }).ShouldHaveValidationErrorFor(query => query.Status);
        new LoansQueryValidator().TestValidate(new LoansQuery { State = "late" }).ShouldHaveValidationErrorFor(query => query.State);
        new UsersQueryValidator().TestValidate(new UsersQuery { Role = "owner" }).ShouldHaveValidationErrorFor(query => query.Role);
        new AssessmentLogQueryValidator().TestValidate(new AssessmentLogQuery { Outcome = "declined" }).ShouldHaveValidationErrorFor(query => query.Outcome);
    }
}
