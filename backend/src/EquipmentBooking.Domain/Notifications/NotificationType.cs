using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Notifications;

public sealed class NotificationType : ReferenceData
{
    private NotificationType()
    {
    }

    public NotificationType(string code, string name)
        : base(code, name)
    {
    }
}
