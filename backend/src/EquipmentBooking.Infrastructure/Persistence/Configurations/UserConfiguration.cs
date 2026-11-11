using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", table =>
        {
            table.HasComment("People who sign in. Personal data: e-mail and full name.");
            table.HasCheckConstraint("ck_users_email_format", "email ~ '^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$'");
            table.HasCheckConstraint("ck_users_failed_sign_in_count", "failed_sign_in_count >= 0");
        });
        builder.Property(user => user.Id).HasComment("Surrogate key.");
        builder.Property(user => user.Email).HasColumnType("citext").HasMaxLength(254).IsRequired()
            .HasComment("Work e-mail address used to sign in, unique without regard to case.");
        builder.HasIndex(user => user.Email).IsUnique().HasDatabaseName("ux_users_email");
        builder.Property(user => user.FullName).HasMaxLength(150).IsRequired().HasComment("Full name shown to managers and administrators.");
        builder.Property(user => user.PasswordHash).HasMaxLength(200).IsRequired()
            .HasComment("PBKDF2-HMAC-SHA512 hash with salt (ASP.NET Core Identity format v3); empty until a password is set.");
        builder.Property(user => user.RoleCode).HasMaxLength(ReferenceDataConfiguration<Role>.CodeLength).IsRequired().HasComment("Role of the user.");
        builder.HasOne<Role>().WithMany().HasForeignKey(user => user.RoleCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(user => user.DepartmentId).HasComment("Department of the user.");
        builder.HasOne(user => user.Department).WithMany().HasForeignKey(user => user.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(user => user.IsActive).HasDefaultValue(true).HasComment("Inactive users cannot sign in.");
        builder.Property(user => user.FailedSignInCount).HasDefaultValue(0).HasComment("Consecutive failed sign ins since the last success or lockout.");
        builder.Property(user => user.LockoutEndsAt).HasComment("Sign in is refused until this time after too many failed attempts.");
        builder.Property(user => user.PrivacyNoticeAcknowledgedAt).HasComment("When the user first saw the privacy notice (acknowledgement, not consent).");
        builder.Property(user => user.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd().HasComment("Creation time.");
        builder.Property(user => user.UpdatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAddOrUpdate().HasComment("Last change time, set by trigger.");
        builder.HasIndex(user => user.DepartmentId).HasDatabaseName("ix_users_department_id");
        builder.HasIndex(user => user.RoleCode).HasDatabaseName("ix_users_role_code");
    }
}
