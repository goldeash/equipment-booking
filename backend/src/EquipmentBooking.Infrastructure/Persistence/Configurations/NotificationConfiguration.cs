using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Notifications;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", table =>
        {
            table.HasComment("In-app notifications; the e-mail columns form an outbox for a future e-mail provider.");
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
        builder.Property(notification => notification.EmailStatus).HasMaxLength(ReferenceDataConfiguration<EmailStatus>.CodeLength).IsRequired().HasComment("Delivery state of the e-mail copy; not_required while no provider is configured.");
        builder.HasOne<EmailStatus>().WithMany().HasForeignKey(notification => notification.EmailStatus).OnDelete(DeleteBehavior.Restrict);
        builder.Property(notification => notification.EmailAttempts).HasDefaultValue(0).HasComment("Number of e-mail delivery attempts.");
        builder.Property(notification => notification.EmailSentAt).HasComment("When the e-mail copy was sent.");
        builder.HasIndex(notification => new { notification.UserId, notification.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("ix_notifications_user_created_at");
        builder.HasIndex(notification => notification.CreatedAt).HasFilter("email_status = 'pending'").HasDatabaseName("ix_notifications_email_pending");
        builder.HasIndex(notification => notification.BookingId).HasDatabaseName("ix_notifications_booking_id");
        builder.HasIndex(notification => notification.NotificationTypeCode).HasDatabaseName("ix_notifications_notification_type_code");
        builder.HasIndex(notification => notification.EmailStatus).HasDatabaseName("ix_notifications_email_status");
    }
}
