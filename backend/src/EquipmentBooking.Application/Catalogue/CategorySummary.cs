namespace EquipmentBooking.Application.Catalogue;

public sealed record CategorySummary(long Id, string Name, int MaxLoanDays, string UsageGuidelines, bool IsSpecialised);
