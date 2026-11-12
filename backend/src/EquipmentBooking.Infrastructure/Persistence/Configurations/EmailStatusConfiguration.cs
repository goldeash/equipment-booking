using EquipmentBooking.Domain.Notifications;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class EmailStatusConfiguration() : ReferenceDataConfiguration<EmailStatus>(
    "email_statuses",
    "Delivery states of the e-mail copy of a notification.")
{
    protected override IEnumerable<EmailStatus> SeedData() =>
    [
        new(EmailStatuses.Pending, "Waiting for delivery"),
        new(EmailStatuses.Sent, "Sent"),
        new(EmailStatuses.Failed, "Delivery failed"),
        new(EmailStatuses.NotRequired, "No e-mail copy is sent"),
    ];
}
