using EquipmentBooking.Domain.Bookings;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class CancellationReasonConfiguration() : ReferenceDataConfiguration<CancellationReason>(
    "cancellation_reasons",
    "Reasons why a booking was cancelled; every cancelled booking has one.")
{
    protected override IEnumerable<CancellationReason> SeedData() =>
    [
        new(CancellationReasonCodes.ByRequester, "Cancelled by the requester"),
        new(CancellationReasonCodes.Maintenance, "Cancelled because the item is out of service for maintenance"),
        new(CancellationReasonCodes.RequestExpired, "No decision was made by the start date"),
        new(CancellationReasonCodes.NotCollected, "The item was not collected by the end date"),
    ];
}
