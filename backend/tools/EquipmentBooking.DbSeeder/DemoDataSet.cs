namespace EquipmentBooking.DbSeeder;

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
