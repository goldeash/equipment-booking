namespace EquipmentBooking.Domain.Approval;

/// <summary>
/// Result of one fixed policy check with the values it compared, for example the item value and the
/// low value threshold, so that a decision can be explained after the settings have changed.
/// Checks of a switch or a flag have no values.
/// </summary>
public sealed record PolicyCheckResult(string CheckTypeCode, bool Passed, decimal? ObservedValue = null, decimal? LimitValue = null);
