using FluentValidation;

namespace EquipmentBooking.Application.Users;

internal static class PasswordRules
{
    public const int MinLength = 12;
    public const int MaxLength = 128;

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Length(MinLength, MaxLength)
            .Must(password => password.Any(char.IsLetter) && password.Any(char.IsDigit))
            .WithMessage("The password must contain letters and digits.");
}
