using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Settings;

/// <summary>Organisation wide settings stored as a single row.</summary>
public sealed class OrganisationSettings
{
    public const short SingletonId = 1;
    public const int MinRetentionDays = 30;

    private OrganisationSettings()
    {
        TimeZone = string.Empty;
    }

    public OrganisationSettings(decimal lowValueThreshold, bool autoApprovalEnabled, int retentionDays, string timeZone, TimeOnly returnTime)
    {
        Id = SingletonId;
        TimeZone = timeZone;
        Update(lowValueThreshold, autoApprovalEnabled, retentionDays, returnTime, null);
    }

    public short Id { get; private set; }

    /// <summary>Items with a value equal to or above the threshold are high value.</summary>
    public decimal LowValueThreshold { get; private set; }

    public bool AutoApprovalEnabled { get; private set; }

    public int RetentionDays { get; private set; }

    /// <summary>IANA time zone of the organisation, for example "Europe/Vilnius".</summary>
    public string TimeZone { get; private set; }

    /// <summary>Local time on the end date by which an item must be returned.</summary>
    public TimeOnly ReturnTime { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public long? UpdatedByUserId { get; private set; }

    public bool IsLowValue(decimal value) => value < LowValueThreshold;

    public void Update(decimal lowValueThreshold, bool autoApprovalEnabled, int retentionDays, TimeOnly returnTime, long? updatedByUserId)
    {
        if (lowValueThreshold <= 0)
        {
            throw new DomainException(DomainErrorCodes.InvalidThreshold, "The low value threshold must be greater than zero.");
        }

        if (retentionDays < MinRetentionDays)
        {
            throw new DomainException(
                DomainErrorCodes.InvalidRetentionPeriod,
                $"The retention period must be at least {MinRetentionDays} days.");
        }

        LowValueThreshold = lowValueThreshold;
        AutoApprovalEnabled = autoApprovalEnabled;
        RetentionDays = retentionDays;
        ReturnTime = returnTime;
        UpdatedByUserId = updatedByUserId;
    }
}
