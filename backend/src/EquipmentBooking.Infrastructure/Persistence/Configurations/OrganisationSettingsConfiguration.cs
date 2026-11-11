using EquipmentBooking.Domain.Settings;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

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
