namespace EquipmentBooking.Infrastructure.Jobs;

/// <summary>Job settings bound from the "Jobs" section.</summary>
public sealed class JobOptions
{
    public const string SectionName = "Jobs";

    /// <summary>Runs the in-process scheduler; disabled in tests.</summary>
    public bool SchedulerEnabled { get; init; } = true;

    /// <summary>Secret for the internal trigger endpoint; the endpoint is disabled when empty.</summary>
    public string? TriggerKey { get; init; }
}
