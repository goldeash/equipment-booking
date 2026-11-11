namespace EquipmentBooking.Application.Loans;

public static class LoanStates
{
    /// <summary>Approved bookings whose start date has come and that can be issued.</summary>
    public const string ReadyToIssue = "ready";

    /// <summary>Issued or overdue loans.</summary>
    public const string Active = "active";

    public const string Overdue = "overdue";

    public static readonly IReadOnlyList<string> All = [ReadyToIssue, Active, Overdue];
}
