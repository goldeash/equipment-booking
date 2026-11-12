namespace EquipmentBooking.Domain.Bookings;

/// <summary>Allowed change from one booking status to another, enforced by the database.</summary>
public sealed class BookingStatusTransition
{
    private BookingStatusTransition()
    {
        FromStatusCode = string.Empty;
        ToStatusCode = string.Empty;
    }

    public BookingStatusTransition(string fromStatusCode, string toStatusCode)
    {
        FromStatusCode = fromStatusCode;
        ToStatusCode = toStatusCode;
    }

    public string FromStatusCode { get; private set; }

    public string ToStatusCode { get; private set; }
}
