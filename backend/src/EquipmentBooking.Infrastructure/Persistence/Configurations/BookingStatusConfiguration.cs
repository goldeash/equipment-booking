using EquipmentBooking.Domain.Bookings;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class BookingStatusConfiguration() : ReferenceDataConfiguration<BookingStatus>(
    "booking_statuses",
    "Statuses of the booking workflow.")
{
    protected override IEnumerable<BookingStatus> SeedData() =>
    [
        new(BookingStatusCodes.Requested, "Requested"),
        new(BookingStatusCodes.Approved, "Approved"),
        new(BookingStatusCodes.Declined, "Declined"),
        new(BookingStatusCodes.Cancelled, "Cancelled"),
        new(BookingStatusCodes.Issued, "Issued"),
        new(BookingStatusCodes.Returned, "Returned"),
        new(BookingStatusCodes.Overdue, "Overdue"),
    ];
}
