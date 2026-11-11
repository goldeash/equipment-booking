namespace EquipmentBooking.Application.Users;

public sealed record UpdateUserRequest(string FullName, string Role, long DepartmentId, bool IsActive);
