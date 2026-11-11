namespace EquipmentBooking.Application.Users;

public sealed record CreateUserRequest(string Email, string FullName, string Role, long DepartmentId, string Password);
