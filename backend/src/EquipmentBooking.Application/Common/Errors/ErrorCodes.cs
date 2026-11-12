namespace EquipmentBooking.Application.Common.Errors;

/// <summary>Error codes raised by the application layer; domain rule codes are in DomainErrorCodes.</summary>
public static class ErrorCodes
{
    public const string ValidationFailed = "validation.failed";
    public const string NotFound = "resource.not_found";
    public const string Forbidden = "access.forbidden";
    public const string ConcurrencyConflict = "concurrency.conflict";
    public const string Unexpected = "server.unexpected";
    public const string RateLimited = "request.rate_limited";

    public const string InvalidCredentials = "auth.invalid_credentials";
    public const string LockedOut = "auth.locked_out";
    public const string InvalidRefreshToken = "auth.invalid_refresh_token";
    public const string Unauthorized = "auth.unauthorized";

    public const string BookingOverlap = "booking.overlap";
    public const string ItemUnderMaintenance = "booking.item_under_maintenance";
    public const string ItemStillOnLoan = "booking.item_still_on_loan";
    public const string ItemOverdue = "booking.item_overdue";
    public const string DuplicateInventoryNumber = "equipment.duplicate_inventory_number";
    public const string ItemHasFutureBookings = "equipment.has_future_bookings";
    public const string DuplicateName = "reference.duplicate_name";
    public const string DuplicateEmail = "user.duplicate_email";
    public const string ConfirmationRequired = "maintenance.confirmation_required";
}
