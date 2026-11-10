using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Application.Common.ReadModels;
using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Maintenance;
using EquipmentBooking.Domain.Notifications;
using EquipmentBooking.Domain.Settings;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace EquipmentBooking.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IAppDbContext
{
    public const string Schema = "app";

    /// <summary>SQLSTATE raised by the maintenance overlap trigger.</summary>
    public const string MaintenanceOverlapSqlState = "EB001";

    private const string BookingOverlapConstraint = "ex_bookings_no_overlap";
    private const string InventoryNumberIndex = "ux_equipment_items_inventory_number";
    private const string UserEmailIndex = "ux_users_email";

    private readonly ICurrentUser? _currentUser;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<Feature> Features => Set<Feature>();

    public DbSet<ItemCondition> ItemConditions => Set<ItemCondition>();

    public DbSet<EquipmentItem> EquipmentItems => Set<EquipmentItem>();

    public DbSet<EquipmentConditionChange> EquipmentConditionChanges => Set<EquipmentConditionChange>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingStatusChange> BookingStatusChanges => Set<BookingStatusChange>();

    public DbSet<ApprovalAssessment> ApprovalAssessments => Set<ApprovalAssessment>();

    public DbSet<RoutingReason> RoutingReasons => Set<RoutingReason>();

    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();

    public DbSet<MaintenanceType> MaintenanceTypes => Set<MaintenanceType>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<OrganisationSettings> OrganisationSettings => Set<OrganisationSettings>();

    public DbSet<RequesterReturnHistory> RequesterReturnHistories => Set<RequesterReturnHistory>();

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        if (Database.CurrentTransaction is not null)
        {
            return await action(cancellationToken);
        }

        var strategy = Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await Database.BeginTransactionAsync(ct);
            await SetActingUserAsync(ct);
            var result = await action(ct);
            await transaction.CommitAsync(ct);
            return result;
        }, cancellationToken);
    }

    public Task LockEquipmentItemAsync(long equipmentItemId, CancellationToken cancellationToken) =>
        Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM app.equipment_items WHERE id = {equipmentItemId} FOR UPDATE",
            cancellationToken);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (Database.CurrentTransaction is not null || !ChangeTracker.HasChanges())
            {
                return await base.SaveChangesAsync(cancellationToken);
            }

            await using IDbContextTransaction transaction = await Database.BeginTransactionAsync(cancellationToken);
            await SetActingUserAsync(cancellationToken);
            var written = await base.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return written;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException postgres)
        {
            throw TranslateDatabaseError(postgres, exception);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.HasPostgresExtension("btree_gist");
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.UseIdentityAlwaysColumns();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>Passes the acting user to database triggers for the current transaction only.</summary>
    private async Task SetActingUserAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUser?.UserIdOrNull;
        if (userId is null)
        {
            return;
        }

        var value = userId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        await Database.ExecuteSqlInterpolatedAsync($"SELECT set_config('app.user_id', {value}, true)", cancellationToken);
    }

    private static Exception TranslateDatabaseError(PostgresException postgres, DbUpdateException exception) =>
        postgres switch
        {
            { SqlState: PostgresErrorCodes.ExclusionViolation, ConstraintName: BookingOverlapConstraint } =>
                new ConflictException(ErrorCodes.BookingOverlap, "The item is already booked for some of the selected dates."),
            { SqlState: MaintenanceOverlapSqlState } =>
                new ConflictException(ErrorCodes.ItemUnderMaintenance, "The item is out of service for maintenance on some of the selected dates."),
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: InventoryNumberIndex } =>
                new ConflictException(ErrorCodes.DuplicateInventoryNumber, "An item with this inventory number already exists."),
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: UserEmailIndex } =>
                new ConflictException(ErrorCodes.DuplicateEmail, "A user with this e-mail address already exists."),
            { SqlState: PostgresErrorCodes.UniqueViolation } =>
                new ConflictException(ErrorCodes.DuplicateName, "A record with the same unique value already exists."),
            _ => exception,
        };
}
