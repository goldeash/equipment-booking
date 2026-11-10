namespace EquipmentBooking.Domain.Common;

/// <summary>Error codes of business rule violations exposed to API clients.</summary>
public static class DomainErrorCodes
{
    public const string InvalidDateRange = "booking.invalid_date_range";
    public const string StartDateInPast = "booking.start_date_in_past";
    public const string InvalidPurpose = "booking.invalid_purpose";
    public const string InvalidTransition = "booking.invalid_transition";
    public const string StartDateNotReached = "booking.start_date_not_reached";
    public const string DeclineCommentRequired = "booking.decline_comment_required";
    public const string DamageCommentRequired = "loan.damage_comment_required";
    public const string ItemRetired = "equipment.retired";
    public const string InvalidValue = "equipment.invalid_value";
    public const string InvalidLoanDuration = "category.invalid_max_loan_days";
    public const string InvalidThreshold = "settings.invalid_threshold";
    public const string InvalidRetentionPeriod = "settings.invalid_retention_period";
    public const string InvalidMaintenancePeriod = "maintenance.invalid_period";
}
