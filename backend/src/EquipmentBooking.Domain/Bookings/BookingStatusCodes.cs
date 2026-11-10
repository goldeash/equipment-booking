namespace EquipmentBooking.Domain.Bookings;

/// <summary>Codes of the booking statuses.</summary>
public static class BookingStatusCodes
{
    public const string Requested = "requested";
    public const string Approved = "approved";
    public const string Declined = "declined";
    public const string Cancelled = "cancelled";
    public const string Issued = "issued";
    public const string Returned = "returned";
    public const string Overdue = "overdue";

    /// <summary>
    /// Statuses in which a booking blocks its item. The same list is part of the exclusion
    /// constraint on the bookings table; a test keeps both in step.
    /// </summary>
    public static readonly IReadOnlyList<string> Blocking = [Requested, Approved, Issued, Overdue];

    /// <summary>Statuses of a loan that has started and not been returned.</summary>
    public static readonly IReadOnlyList<string> OnLoan = [Issued, Overdue];

    public static readonly IReadOnlyList<string> All = [Requested, Approved, Declined, Cancelled, Issued, Returned, Overdue];
}
