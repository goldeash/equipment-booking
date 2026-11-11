namespace EquipmentBooking.Application.Auth;

/// <summary>Profile of the signed in user.</summary>
public sealed record CurrentUserResponse(
    long Id,
    string Email,
    string FullName,
    string Role,
    long DepartmentId,
    string DepartmentName,
    bool PrivacyNoticeAcknowledged);
