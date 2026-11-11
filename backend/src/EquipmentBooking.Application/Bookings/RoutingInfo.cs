namespace EquipmentBooking.Application.Bookings;

public sealed record RoutingInfo(string Outcome, string PurposeResult, IReadOnlyList<string> Reasons, string? Summary, DateTimeOffset AssessedAt);
