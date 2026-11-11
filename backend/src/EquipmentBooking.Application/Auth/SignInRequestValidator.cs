using FluentValidation;

namespace EquipmentBooking.Application.Auth;

public sealed class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().MaximumLength(254).EmailAddress();
        RuleFor(request => request.Password).NotEmpty().MaximumLength(200);
    }
}
