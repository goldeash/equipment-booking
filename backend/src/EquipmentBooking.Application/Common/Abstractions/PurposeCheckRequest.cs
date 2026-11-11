namespace EquipmentBooking.Application.Common.Abstractions;

/// <summary>Input of the purpose check: only the purpose text and the category data, never the requester.</summary>
public sealed record PurposeCheckRequest(string Purpose, string CategoryName, string UsageGuidelines, string ItemName);
