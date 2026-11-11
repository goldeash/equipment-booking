namespace EquipmentBooking.DbSeeder;

internal sealed record UserRow(long Id, string Email, string FullName, string Role, long DepartmentId, DateTimeOffset CreatedAt);
