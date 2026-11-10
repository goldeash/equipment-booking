using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Common;
using EquipmentBooking.Domain.Maintenance;
using EquipmentBooking.Domain.Notifications;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

/// <summary>Shared mapping of fixed reference tables with a text code as primary key.</summary>
internal abstract class ReferenceDataConfiguration<T> : IEntityTypeConfiguration<T>
    where T : ReferenceData
{
    public const int CodeLength = 40;
    public const int NameLength = 100;

    private readonly string _table;
    private readonly string _comment;

    protected ReferenceDataConfiguration(string table, string comment)
    {
        _table = table;
        _comment = comment;
    }

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(_table, table => table.HasComment(_comment));
        builder.HasKey(item => item.Code);
        builder.Property(item => item.Code).HasMaxLength(CodeLength).HasComment("Stable code used by the application and constraints.");
        builder.Property(item => item.Name).HasMaxLength(NameLength).IsRequired().HasComment("Display name in English.");
        builder.HasData(SeedData());
    }

    protected abstract IEnumerable<T> SeedData();
}

internal sealed class RoleConfiguration() : ReferenceDataConfiguration<Role>("roles", "Fixed list of user roles.")
{
    protected override IEnumerable<Role> SeedData() =>
    [
        new(RoleCodes.Employee, "Employee"),
        new(RoleCodes.Manager, "Department manager"),
        new(RoleCodes.Administrator, "Administrator"),
    ];
}

internal sealed class ItemStatusConfiguration() : ReferenceDataConfiguration<ItemStatus>(
    "item_statuses",
    "Stored statuses of equipment items. Out of service is derived from active maintenance records.")
{
    protected override IEnumerable<ItemStatus> SeedData() =>
    [
        new(ItemStatusCodes.Available, "Available"),
        new(ItemStatusCodes.Retired, "Retired"),
    ];
}

internal sealed class ItemConditionConfiguration() : ReferenceDataConfiguration<ItemCondition>(
    "item_conditions",
    "Physical condition of an item, ordered from best to worst.")
{
    public override void Configure(EntityTypeBuilder<ItemCondition> builder)
    {
        base.Configure(builder);
        builder.Property(item => item.SortOrder).HasComment("Display order, 1 is the best condition.");
    }

    protected override IEnumerable<ItemCondition> SeedData() =>
    [
        new(ItemConditionCodes.New, "New", 1),
        new(ItemConditionCodes.Good, "Good", 2),
        new(ItemConditionCodes.Fair, "Fair", 3),
        new(ItemConditionCodes.Damaged, "Damaged", 4),
    ];
}

internal sealed class BookingStatusConfiguration() : ReferenceDataConfiguration<BookingStatus>(
    "booking_statuses",
    "Statuses of the booking workflow.")
{
    protected override IEnumerable<BookingStatus> SeedData() =>
    [
        new(BookingStatusCodes.Requested, "Requested"),
        new(BookingStatusCodes.Approved, "Approved"),
        new(BookingStatusCodes.Declined, "Declined"),
        new(BookingStatusCodes.Cancelled, "Cancelled"),
        new(BookingStatusCodes.Issued, "Issued"),
        new(BookingStatusCodes.Returned, "Returned"),
        new(BookingStatusCodes.Overdue, "Overdue"),
    ];
}

internal sealed class DecisionSourceConfiguration() : ReferenceDataConfiguration<DecisionSource>(
    "decision_sources",
    "Who took a decision on a request.")
{
    protected override IEnumerable<DecisionSource> SeedData() =>
    [
        new(DecisionSourceCodes.Manager, "Department manager"),
        new(DecisionSourceCodes.AiAssistant, "AI assistant"),
    ];
}

internal sealed class RoutingReasonConfiguration() : ReferenceDataConfiguration<RoutingReason>(
    "routing_reasons",
    "Reasons why a request is routed to the department manager instead of being approved automatically.")
{
    protected override IEnumerable<RoutingReason> SeedData() =>
    [
        new(RoutingReasonCodes.HighValue, "The item is high value"),
        new(RoutingReasonCodes.Specialised, "The item or its category is specialised"),
        new(RoutingReasonCodes.DurationExceeded, "The loan exceeds the maximum duration of the category"),
        new(RoutingReasonCodes.OrganisationAutoApprovalOff, "Automatic approval is switched off for the organisation"),
        new(RoutingReasonCodes.CategoryAutoApprovalOff, "Automatic approval is switched off for the category"),
        new(RoutingReasonCodes.PurposeNotConfirmed, "The assistant could not confirm the purpose"),
        new(RoutingReasonCodes.AssistantUnavailable, "The AI assistant is not available"),
    ];
}

internal sealed class MaintenanceTypeConfiguration() : ReferenceDataConfiguration<MaintenanceType>(
    "maintenance_types",
    "Types of maintenance work.")
{
    protected override IEnumerable<MaintenanceType> SeedData() =>
    [
        new(MaintenanceTypeCodes.Inspection, "Inspection"),
        new(MaintenanceTypeCodes.Repair, "Repair"),
        new(MaintenanceTypeCodes.Calibration, "Calibration"),
        new(MaintenanceTypeCodes.Cleaning, "Cleaning"),
    ];
}

internal sealed class NotificationTypeConfiguration() : ReferenceDataConfiguration<NotificationType>(
    "notification_types",
    "Types of in-app notifications.")
{
    protected override IEnumerable<NotificationType> SeedData() =>
    [
        new(NotificationTypeCodes.RequestApproved, "Request approved"),
        new(NotificationTypeCodes.RequestDeclined, "Request declined"),
        new(NotificationTypeCodes.ReturnReminder, "Return reminder"),
        new(NotificationTypeCodes.LoanOverdue, "Loan overdue"),
        new(NotificationTypeCodes.BookingCancelledByMaintenance, "Booking cancelled because of maintenance"),
    ];
}
