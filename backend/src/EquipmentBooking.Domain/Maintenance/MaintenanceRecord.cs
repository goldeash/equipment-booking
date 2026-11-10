using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Maintenance;

/// <summary>Maintenance or repair of an item. An out of service record blocks bookings in its period.</summary>
public sealed class MaintenanceRecord : Entity
{
    private MaintenanceRecord()
    {
        MaintenanceTypeCode = string.Empty;
        Description = string.Empty;
    }

    public MaintenanceRecord(
        long equipmentItemId,
        string maintenanceTypeCode,
        DateOnly startDate,
        DateOnly? endDate,
        bool takesOutOfService,
        string description,
        long recordedByUserId)
    {
        if (endDate is { } end && end < startDate)
        {
            throw new DomainException(DomainErrorCodes.InvalidMaintenancePeriod, "The end date must not be before the start date.");
        }

        EquipmentItemId = equipmentItemId;
        MaintenanceTypeCode = maintenanceTypeCode;
        StartDate = startDate;
        EndDate = endDate;
        TakesOutOfService = takesOutOfService;
        Description = description.Trim();
        RecordedByUserId = recordedByUserId;
    }

    public long EquipmentItemId { get; private set; }

    public string MaintenanceTypeCode { get; private set; }

    public DateOnly StartDate { get; private set; }

    /// <summary>Last day of the maintenance; null while the end is not known.</summary>
    public DateOnly? EndDate { get; private set; }

    public bool TakesOutOfService { get; private set; }

    public string Description { get; private set; }

    public long RecordedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool Overlaps(DateRange period) =>
        StartDate <= period.End && (EndDate is null || EndDate.Value >= period.Start);
}

/// <summary>Codes of maintenance types.</summary>
public static class MaintenanceTypeCodes
{
    public const string Inspection = "inspection";
    public const string Repair = "repair";
    public const string Calibration = "calibration";
    public const string Cleaning = "cleaning";
}

public sealed class MaintenanceType : ReferenceData
{
    private MaintenanceType()
    {
    }

    public MaintenanceType(string code, string name)
        : base(code, name)
    {
    }
}
