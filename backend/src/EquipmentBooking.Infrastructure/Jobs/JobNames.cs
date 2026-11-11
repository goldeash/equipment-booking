namespace EquipmentBooking.Infrastructure.Jobs;

/// <summary>Names of the scheduled jobs, also used by the internal trigger endpoint.</summary>
public static class JobNames
{
    public const string OverdueLoans = "overdue-loans";
    public const string ReturnReminders = "return-reminders";
    public const string DataRetention = "data-retention";
    public const string ExpiredBookings = "expired-bookings";

    public static readonly IReadOnlyList<string> All = [OverdueLoans, ReturnReminders, DataRetention, ExpiredBookings];
}
