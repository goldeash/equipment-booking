using EquipmentBooking.Domain.Users;
using FluentValidation;

namespace EquipmentBooking.Application.Users;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Role).Must(role => RoleCodes.All.Contains(role))
            .WithMessage($"'Role' must be one of: {string.Join(", ", RoleCodes.All)}.");
        RuleFor(request => request.DepartmentId).GreaterThan(0);
    }
}
