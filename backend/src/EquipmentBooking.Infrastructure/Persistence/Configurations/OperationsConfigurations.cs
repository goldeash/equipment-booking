using EquipmentBooking.Application.Common.ReadModels;
using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Maintenance;
using EquipmentBooking.Domain.Notifications;
using EquipmentBooking.Domain.Settings;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class ApprovalAssessmentConfiguration : IEntityTypeConfiguration<ApprovalAssessment>
{
    public void Configure(EntityTypeBuilder<ApprovalAssessment> builder)
    {
        builder.ToTable("approval_assessments", table =>
        {
            table.HasComment("Decision log of the approval route: result of every policy check, the purpose check and the outcome.");
            table.HasCheckConstraint("ck_approval_assessments_purpose_result", "purpose_result IN ('pending', 'match', 'no_match', 'unavailable', 'not_run')");
            table.HasCheckConstraint("ck_approval_assessments_outcome", "outcome IN ('pending', 'approved', 'routed')");
            table.HasCheckConstraint("ck_approval_assessments_approved_needs_match", "outcome <> 'approved' OR purpose_result = 'match'");
        });
        builder.Property(assessment => assessment.Id).HasComment("Surrogate key.");
        builder.Property(assessment => assessment.BookingId).HasComment("Assessed booking.");
        builder.HasOne<Booking>().WithMany().HasForeignKey(assessment => assessment.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(assessment => assessment.AssessedAt).HasComment("Time of the assessment.");
        builder.Property(assessment => assessment.ValueCheckPassed).HasComment("The item value is below the low value threshold.");
        builder.Property(assessment => assessment.SpecialisedCheckPassed).HasComment("Neither the item nor its category is specialised.");
        builder.Property(assessment => assessment.DurationCheckPassed).HasComment("The loan does not exceed the maximum duration of the category.");
        builder.Property(assessment => assessment.OrganisationAutoApprovalEnabled).HasComment("Automatic approval was switched on for the organisation.");
        builder.Property(assessment => assessment.CategoryAutoApprovalEnabled).HasComment("Automatic approval was switched on for the category.");
        builder.Property(assessment => assessment.PurposeResult).HasMaxLength(20).IsRequired().HasComment("Result of the purpose check by the language model.");
        builder.Property(assessment => assessment.Outcome).HasMaxLength(20).IsRequired().HasComment("pending, approved or routed to the manager.");
        builder.Property(assessment => assessment.Summary).HasMaxLength(1000).HasComment("Short summary for the manager prepared by the assistant.");
        builder.Property(assessment => assessment.ModelName).HasMaxLength(100).HasComment("Language model used for the purpose check.");
        builder.Property(assessment => assessment.PromptVersion).HasMaxLength(20).HasComment("Version of the prompt template used.");
        builder.Ignore(assessment => assessment.IsPending);
        builder.HasMany(assessment => assessment.Reasons).WithOne().HasForeignKey(reason => reason.AssessmentId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(assessment => assessment.Reasons).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(assessment => assessment.BookingId).HasDatabaseName("ix_approval_assessments_booking_id");
        builder.HasIndex(assessment => assessment.AssessedAt).HasDatabaseName("ix_approval_assessments_assessed_at");
    }
}

internal sealed class ApprovalAssessmentReasonConfiguration : IEntityTypeConfiguration<ApprovalAssessmentReason>
{
    public void Configure(EntityTypeBuilder<ApprovalAssessmentReason> builder)
    {
        builder.ToTable("approval_assessment_reasons", table => table.HasComment("Many-to-many link between assessments and every reason why the request was routed."));
        builder.HasKey(reason => new { reason.AssessmentId, reason.RoutingReasonCode });
        builder.Property(reason => reason.AssessmentId).HasComment("Assessment.");
        builder.Property(reason => reason.RoutingReasonCode).HasMaxLength(ReferenceDataConfiguration<RoutingReason>.CodeLength).HasComment("Reason for routing.");
        builder.HasOne<RoutingReason>().WithMany().HasForeignKey(reason => reason.RoutingReasonCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(reason => reason.RoutingReasonCode).HasDatabaseName("ix_approval_assessment_reasons_routing_reason_code");
    }
}

internal sealed class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.ToTable("maintenance_records", table =>
        {
            table.HasComment("Maintenance and repairs of items; an out of service record blocks bookings in its period.");
            table.HasCheckConstraint("ck_maintenance_records_dates", "end_date IS NULL OR end_date >= start_date");
        });
        builder.Property(record => record.Id).HasComment("Surrogate key.");
        builder.Property(record => record.EquipmentItemId).HasComment("Maintained item.");
        builder.HasOne<EquipmentItem>().WithMany().HasForeignKey(record => record.EquipmentItemId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(record => record.MaintenanceTypeCode).HasMaxLength(ReferenceDataConfiguration<MaintenanceType>.CodeLength).IsRequired().HasComment("Type of work.");
        builder.HasOne<MaintenanceType>().WithMany().HasForeignKey(record => record.MaintenanceTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(record => record.StartDate).HasComment("First day of the maintenance.");
        builder.Property(record => record.EndDate).HasComment("Last day of the maintenance; null while open ended.");
        builder.Property(record => record.TakesOutOfService).HasComment("The item cannot be booked during the period.");
        builder.Property(record => record.Description).HasMaxLength(2000).IsRequired().HasComment("What was done or found.");
        builder.Property(record => record.RecordedByUserId).HasComment("Administrator who recorded the entry.");
        builder.HasOne<User>().WithMany().HasForeignKey(record => record.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(record => record.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd().HasComment("Creation time.");
        builder.HasIndex(record => new { record.EquipmentItemId, record.StartDate }).HasDatabaseName("ix_maintenance_records_item_start_date");
    }
}

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", table =>
        {
            table.HasComment("In-app notifications; the e-mail columns form an outbox for a future e-mail provider.");
            table.HasCheckConstraint("ck_notifications_email_status", "email_status IN ('pending', 'sent', 'failed', 'not_required')");
            table.HasCheckConstraint("ck_notifications_email_attempts", "email_attempts >= 0");
        });
        builder.Property(notification => notification.Id).HasComment("Surrogate key.");
        builder.Property(notification => notification.UserId).HasComment("Recipient.");
        builder.HasOne<User>().WithMany().HasForeignKey(notification => notification.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(notification => notification.NotificationTypeCode).HasMaxLength(ReferenceDataConfiguration<NotificationType>.CodeLength).IsRequired().HasComment("Type of notification.");
        builder.HasOne<NotificationType>().WithMany().HasForeignKey(notification => notification.NotificationTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(notification => notification.BookingId).HasComment("Related booking, if any.");
        builder.HasOne<Booking>().WithMany().HasForeignKey(notification => notification.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(notification => notification.Title).HasMaxLength(150).IsRequired().HasComment("Short title.");
        builder.Property(notification => notification.Body).HasMaxLength(1000).IsRequired().HasComment("Message text.");
        builder.Property(notification => notification.CreatedAt).HasComment("Creation time.");
        builder.Property(notification => notification.ReadAt).HasComment("When the recipient marked it as read.");
        builder.Property(notification => notification.EmailStatus).HasMaxLength(20).IsRequired().HasComment("Delivery state of the e-mail copy; not_required while no provider is configured.");
        builder.Property(notification => notification.EmailAttempts).HasDefaultValue(0).HasComment("Number of e-mail delivery attempts.");
        builder.Property(notification => notification.EmailSentAt).HasComment("When the e-mail copy was sent.");
        builder.HasIndex(notification => new { notification.UserId, notification.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("ix_notifications_user_created_at");
        builder.HasIndex(notification => notification.CreatedAt).HasFilter("email_status = 'pending'").HasDatabaseName("ix_notifications_email_pending");
        builder.HasIndex(notification => notification.BookingId).HasDatabaseName("ix_notifications_booking_id");
    }
}

internal sealed class OrganisationSettingsConfiguration : IEntityTypeConfiguration<OrganisationSettings>
{
    public void Configure(EntityTypeBuilder<OrganisationSettings> builder)
    {
        builder.ToTable("organisation_settings", table =>
        {
            table.HasComment("Organisation wide settings, exactly one row.");
            table.HasCheckConstraint("ck_organisation_settings_singleton", "id = 1");
            table.HasCheckConstraint("ck_organisation_settings_threshold", "low_value_threshold > 0");
            table.HasCheckConstraint("ck_organisation_settings_retention", "retention_days >= 30");
        });
        builder.Property(settings => settings.Id).ValueGeneratedNever().HasComment("Always 1.");
        builder.Property(settings => settings.LowValueThreshold).HasPrecision(12, 2).HasComment("Items with a value equal to or above it are high value (EUR).");
        builder.Property(settings => settings.AutoApprovalEnabled).HasComment("Organisation wide switch for automatic approval.");
        builder.Property(settings => settings.RetentionDays).HasComment("Days after the return after which personal data of a loan is anonymised.");
        builder.Property(settings => settings.TimeZone).HasMaxLength(64).IsRequired().HasComment("IANA time zone of the organisation.");
        builder.Property(settings => settings.ReturnTime).HasComment("Local time on the end date by which items must be returned.");
        builder.Property(settings => settings.UpdatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAddOrUpdate().HasComment("Last change time, set by trigger.");
        builder.Property(settings => settings.UpdatedByUserId).HasComment("Administrator who changed the settings last.");
        builder.HasOne<User>().WithMany().HasForeignKey(settings => settings.UpdatedByUserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasData(new
        {
            Id = OrganisationSettings.SingletonId,
            LowValueThreshold = 500.00m,
            AutoApprovalEnabled = true,
            RetentionDays = 365,
            TimeZone = "Europe/Vilnius",
            ReturnTime = new TimeOnly(18, 0),
        });
    }
}

internal sealed class RequesterReturnHistoryConfiguration : IEntityTypeConfiguration<RequesterReturnHistory>
{
    public void Configure(EntityTypeBuilder<RequesterReturnHistory> builder)
    {
        builder.HasNoKey();
        builder.ToView("v_requester_return_history");
    }
}
