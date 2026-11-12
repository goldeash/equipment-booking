using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Notifications;

public sealed class EmailStatus : ReferenceData
{
    private EmailStatus()
    {
    }

    public EmailStatus(string code, string name)
        : base(code, name)
    {
    }
}
