namespace EquipmentBooking.DbSeeder;

internal sealed record DepartmentRow(long Id, string Name);

internal sealed record LocationRow(long Id, string Name);

internal sealed record FeatureRow(long Id, string Name);

internal sealed record CategoryRow(long Id, CategorySpec Spec);

internal sealed record UserRow(long Id, string Email, string FullName, string Role, long DepartmentId, DateTimeOffset CreatedAt);

internal sealed class ItemRow
{
    public required long Id { get; init; }

    public required string InventoryNumber { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required CategoryRow Category { get; init; }

    public required long LocationId { get; init; }

    public required decimal Value { get; init; }

    public required string InitialCondition { get; init; }

    public required string Condition { get; set; }

    public required IReadOnlyList<long> FeatureIds { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}

internal sealed class BookingRow
{
    public required long Id { get; init; }

    public required ItemRow Item { get; init; }

    public required UserRow Requester { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateOnly EndDate { get; init; }

    public required string Purpose { get; init; }

    public required string Status { get; set; }

    public required DateTimeOffset CreatedAt { get; init; }
}

internal sealed record DecisionRow(long Id, long BookingId, long ManagerId, string Outcome, string? Comment, DateTimeOffset DecidedAt);

internal sealed record LoanRow(
    long BookingId,
    DateTimeOffset IssuedAt,
    long IssuedBy,
    DateTimeOffset DueAt,
    DateTimeOffset? ReturnedAt,
    long? ReceivedBy,
    string? ReturnCondition,
    string? ReturnComment);

internal sealed record AssessmentRow(
    long Id,
    long BookingId,
    DateTimeOffset AssessedAt,
    bool ValuePassed,
    bool SpecialisedPassed,
    bool DurationPassed,
    string PurposeResult,
    IReadOnlyList<string> Reasons);

internal sealed record MaintenanceRow(
    long Id,
    long ItemId,
    string Type,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool TakesOutOfService,
    string Description,
    long RecordedBy,
    DateTimeOffset CreatedAt);

internal sealed record StatusChangeRow(long BookingId, string? From, string To, DateTimeOffset ChangedAt, long? ChangedBy);

internal sealed record ConditionChangeRow(long ItemId, string? From, string To, DateTimeOffset ChangedAt, long? ChangedBy);

internal sealed record NotificationRow(
    long UserId,
    string Type,
    long? BookingId,
    string Title,
    string Body,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);

/// <summary>Complete generated demo data set.</summary>
internal sealed class DemoDataSet
{
    public List<DepartmentRow> Departments { get; } = [];

    public List<LocationRow> Locations { get; } = [];

    public List<FeatureRow> Features { get; } = [];

    public List<CategoryRow> Categories { get; } = [];

    public List<UserRow> Users { get; } = [];

    public List<ItemRow> Items { get; } = [];

    public List<BookingRow> Bookings { get; } = [];

    public List<DecisionRow> Decisions { get; } = [];

    public List<LoanRow> Loans { get; } = [];

    public List<AssessmentRow> Assessments { get; } = [];

    public List<MaintenanceRow> Maintenance { get; } = [];

    public List<StatusChangeRow> StatusChanges { get; } = [];

    public List<ConditionChangeRow> ConditionChanges { get; } = [];

    public List<NotificationRow> Notifications { get; } = [];
}
