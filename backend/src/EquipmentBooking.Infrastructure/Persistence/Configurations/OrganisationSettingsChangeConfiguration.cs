using EquipmentBooking.Domain.Settings;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class OrganisationSettingsChangeConfiguration : IEntityTypeConfiguration<OrganisationSettingsChange>
{
    public void Configure(EntityTypeBuilder<OrganisationSettingsChange> builder)
    {
        builder.ToTable("organisation_settings_history", table => table.HasComment("Every version of the organisation settings, written by trigger."));
        builder.Property(change => change.Id).HasComment("Surrogate key.");
        builder.Property(change => change.ChangedAt).HasDefaultValueSql("now()").HasComment("When this version came into force.");
        builder.Property(change => change.ChangedByUserId).HasComment("Administrator who made the change; null for the initial settings and after a user was removed.");
        builder.HasOne<User>().WithMany().HasForeignKey(change => change.ChangedByUserId).OnDelete(DeleteBehavior.SetNull);
        builder.Property(change => change.LowValueThreshold).HasPrecision(12, 2).HasComment("Low value threshold in EUR.");
        builder.Property(change => change.AutoApprovalEnabled).HasComment("Organisation wide switch for automatic approval.");
        builder.Property(change => change.RetentionDays).HasComment("Retention period of personal data of loans in days.");
        builder.Property(change => change.TimeZone).HasMaxLength(64).IsRequired().HasComment("IANA time zone of the organisation.");
        builder.Property(change => change.ReturnTime).HasComment("Local return time on the end date.");
        builder.HasIndex(change => change.ChangedAt).HasDatabaseName("ix_organisation_settings_history_changed_at");
        builder.HasIndex(change => change.ChangedByUserId).HasDatabaseName("ix_organisation_settings_history_changed_by_user_id");
    }
}
