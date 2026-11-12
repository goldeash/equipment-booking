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

    /// <summary>
    /// Allowed status changes of a booking. The same pairs are stored in the table
    /// booking_status_transitions, which the database enforces; a test keeps both in step.
    /// A date change sends an approved booking back to Requested.
    /// </summary>
    public static readonly IReadOnlyList<(string From, string To)> Transitions =
    [
        (Requested, Approved),
        (Requested, Declined),
        (Requested, Cancelled),
        (Approved, Requested),
        (Approved, Issued),
        (Approved, Cancelled),
        (Issued, Overdue),
        (Issued, Returned),
        (Overdue, Returned),
    ];

    public static bool CanChange(string from, string to) => Transitions.Contains((from, to));
}
