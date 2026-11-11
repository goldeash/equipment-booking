using EquipmentBooking.Domain.Notifications;
using Shouldly;

namespace EquipmentBooking.Domain.Tests;

public sealed class NotificationTests
{
    [Fact]
    public void Email_status_follows_the_email_setting()
    {
        new Notification(1, NotificationTypeCodes.RequestApproved, 5, "Title", "Body", false, TestData.Now).EmailStatus.ShouldBe(EmailStatuses.NotRequired);
        new Notification(1, NotificationTypeCodes.RequestApproved, 5, "Title", "Body", true, TestData.Now).EmailStatus.ShouldBe(EmailStatuses.Pending);
    }

    [Fact]
    public void Read_time_and_delivery_attempts_are_recorded()
    {
        var notification = new Notification(1, NotificationTypeCodes.LoanOverdue, null, "Title", "Body", true, TestData.Now);

        notification.MarkRead(TestData.Now);
        notification.MarkRead(TestData.Now.AddHours(1));
        notification.MarkEmailFailed();
        notification.MarkEmailSent(TestData.Now);

        notification.ReadAt.ShouldBe(TestData.Now);
        notification.EmailAttempts.ShouldBe(2);
        notification.EmailStatus.ShouldBe(EmailStatuses.Sent);
    }
}
