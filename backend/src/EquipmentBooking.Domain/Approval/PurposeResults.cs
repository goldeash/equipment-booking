namespace EquipmentBooking.Domain.Approval;

/// <summary>Result of the language model check of the stated purpose.</summary>
public static class PurposeResults
{
    public const string Pending = "pending";
    public const string Match = "match";
    public const string NoMatch = "no_match";
    public const string Unavailable = "unavailable";
    public const string NotRun = "not_run";
}
