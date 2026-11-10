using System.Reflection;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Tests;

internal static class TestData
{
    public static readonly DateOnly Today = new(2026, 11, 10);
    public static readonly DateTimeOffset Now = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);

    public static EquipmentItem Item(decimal value = 300m, bool isSpecialised = false, long id = 7)
    {
        var item = new EquipmentItem("EQ-CAM-001", "Canon EOS R50", "Camera", 1, 1, value, ItemConditionCodes.Good, isSpecialised);
        SetId(item, id);
        return item;
    }

    public static Booking RequestedBooking(int startInDays = 1, int lengthInDays = 3, long requesterId = 11)
    {
        var start = Today.AddDays(startInDays);
        var booking = Booking.Request(Item(), requesterId, new DateRange(start, start.AddDays(lengthInDays - 1)), "Recording an interview", Today);
        SetId(booking, 100);
        return booking;
    }

    public static Booking ApprovedBooking(int startInDays = 0)
    {
        var booking = RequestedBooking(startInDays);
        booking.ApproveByManager(2, null, Now);
        return booking;
    }

    public static Booking IssuedBooking()
    {
        var booking = ApprovedBooking();
        booking.Issue(1, Today, Now.AddDays(2), Now);
        return booking;
    }

    public static void SetId(Entity entity, long id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id), BindingFlags.Public | BindingFlags.Instance)!.SetValue(entity, id);
}
