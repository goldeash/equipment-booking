namespace EquipmentBooking.Application.Approval;

/// <summary>Result of one policy check with the compared values, for example the item value and the threshold.</summary>
public sealed record AssessmentCheckInfo(string Code, string Name, bool Passed, decimal? ObservedValue, decimal? LimitValue);
