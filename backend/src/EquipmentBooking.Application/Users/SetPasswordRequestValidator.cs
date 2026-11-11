using FluentValidation;

namespace EquipmentBooking.Application.Users;

public sealed class SetPasswordRequestValidator : AbstractValidator<SetPasswordRequest>
{
    public SetPasswordRequestValidator()
    {
        RuleFor(request => request.Password).StrongPassword();
    }
}
