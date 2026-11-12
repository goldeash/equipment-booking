namespace EquipmentBooking.Domain.Settings;

/// <summary>Snapshot of the organisation settings after a change, written by a database trigger.</summary>
public sealed class OrganisationSettingsChange
{
    private OrganisationSettingsChange()
    {
        TimeZone = string.Empty;
    }

    public long Id { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }

    public long? ChangedByUserId { get; private set; }

    public decimal LowValueThreshold { get; private set; }

    public bool AutoApprovalEnabled { get; private set; }

    public int RetentionDays { get; private set; }

    public string TimeZone { get; private set; }

    public TimeOnly ReturnTime { get; private set; }
}
