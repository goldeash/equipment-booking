namespace EquipmentBooking.DbSeeder;

internal sealed record DecisionRow(long Id, long BookingId, long ManagerId, string Outcome, string? Comment, DateTimeOffset DecidedAt);
