namespace EquipmentBooking.DbSeeder;

internal sealed record CheckRow(string CheckTypeCode, bool Passed, decimal? ObservedValue, decimal? LimitValue);
