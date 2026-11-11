namespace EquipmentBooking.Application.Users;

public sealed record UserResponse(
    long Id,
    string Email,
    string FullName,
    string Role,
    long DepartmentId,
    string DepartmentName,
    bool IsActive,
    DateTimeOffset CreatedAt);
