namespace EquipmentBooking.Application.Catalogue;

/// <summary>Event in the history of an item (US-32).</summary>
public sealed record EquipmentHistoryEvent(DateTimeOffset OccurredAt, string Kind, string Summary, long? BookingId);
