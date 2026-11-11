namespace EquipmentBooking.Application.Loans;

public sealed record ReturnLoanRequest(string ConditionCode, string? Comment);
