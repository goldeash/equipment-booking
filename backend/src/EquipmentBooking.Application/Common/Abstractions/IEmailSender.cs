namespace EquipmentBooking.Application.Common.Abstractions;

/// <summary>Port for sending notification e-mails; no provider is connected yet.</summary>
public interface IEmailSender
{
    Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken);
}
