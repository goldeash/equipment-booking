using EquipmentBooking.Application.Common.ReadModels;
using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Maintenance;
using EquipmentBooking.Domain.Notifications;
using EquipmentBooking.Domain.Settings;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Common.Abstractions;

/// <summary>Unit of work over the application database.</summary>
public interface IAppDbContext
{
    DbSet<User> Users { get; }

    DbSet<Department> Departments { get; }

    DbSet<Role> Roles { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Category> Categories { get; }

    DbSet<Location> Locations { get; }

    DbSet<Feature> Features { get; }

    DbSet<ItemCondition> ItemConditions { get; }

    DbSet<EquipmentItem> EquipmentItems { get; }

    DbSet<EquipmentConditionChange> EquipmentConditionChanges { get; }

    DbSet<Booking> Bookings { get; }

    DbSet<BookingStatusChange> BookingStatusChanges { get; }

    DbSet<ApprovalAssessment> ApprovalAssessments { get; }

    DbSet<RoutingReason> RoutingReasons { get; }

    DbSet<ApprovalCheckType> ApprovalCheckTypes { get; }

    /// <summary>Routing reasons derived from failed checks and the purpose result, read from the view v_assessment_routing_reasons.</summary>
    DbSet<AssessmentRoutingReason> AssessmentRoutingReasons { get; }

    DbSet<CancellationReason> CancellationReasons { get; }

    DbSet<MaintenanceRecord> MaintenanceRecords { get; }

    DbSet<MaintenanceType> MaintenanceTypes { get; }

    DbSet<Notification> Notifications { get; }

    DbSet<OrganisationSettings> OrganisationSettings { get; }

    /// <summary>Loan statistics of every requester, read from the view v_requester_return_history.</summary>
    DbSet<RequesterReturnHistory> RequesterReturnHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Runs the action in one database transaction and commits it when the action succeeds.</summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken);

    /// <summary>Locks the item row until the end of the current transaction (SELECT ... FOR UPDATE).</summary>
    Task LockEquipmentItemAsync(long equipmentItemId, CancellationToken cancellationToken);
}
