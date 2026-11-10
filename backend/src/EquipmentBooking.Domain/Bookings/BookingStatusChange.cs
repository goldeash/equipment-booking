using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Bookings;

/// <summary>Status change of a booking, written by a database trigger.</summary>
public sealed class BookingStatusChange : Entity
{
    private BookingStatusChange()
    {
        ToStatusCode = string.Empty;
    }

    public long BookingId { get; private set; }

    public string? FromStatusCode { get; private set; }

    public string ToStatusCode { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }

    public long? ChangedByUserId { get; private set; }
}
