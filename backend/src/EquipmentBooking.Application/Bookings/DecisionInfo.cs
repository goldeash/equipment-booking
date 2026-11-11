namespace EquipmentBooking.Application.Bookings;

/// <summary>Who took the current decision: a manager (with name) or the AI assistant.</summary>
public sealed record DecisionInfo(string Source, string Outcome, string? DecidedByName, string? Comment, DateTimeOffset DecidedAt);
