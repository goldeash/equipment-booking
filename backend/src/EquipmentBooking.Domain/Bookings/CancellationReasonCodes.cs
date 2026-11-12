namespace EquipmentBooking.Domain.Bookings;

/// <summary>Why a booking was cancelled; every cancelled booking has exactly one reason.</summary>
public static class CancellationReasonCodes
{
    public const string ByRequester = "by_requester";
    public const string Maintenance = "maintenance";
    public const string RequestExpired = "request_expired";
    public const string NotCollected = "not_collected";
}
