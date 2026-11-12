namespace EquipmentBooking.Application.Bookings;

/// <summary>Why a cancelled booking was cancelled, for example by the requester or because no decision was made in time.</summary>
public sealed record CancellationInfo(string Code, string Name);
