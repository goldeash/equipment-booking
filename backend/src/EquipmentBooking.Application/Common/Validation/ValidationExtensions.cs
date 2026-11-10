using FluentValidation;

namespace EquipmentBooking.Application.Common.Validation;

public static class ValidationExtensions
{
    /// <summary>Validates the request and throws a ValidationException with all failures.</summary>
    public static async Task EnsureValidAsync<T>(this IValidator<T> validator, T request, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    /// <summary>Rules shared by optional free text fields.</summary>
    public static IRuleBuilderOptions<T, string?> OptionalText<T>(this IRuleBuilder<T, string?> rule, int maxLength) =>
        rule.MaximumLength(maxLength);

    /// <summary>Rules shared by required text fields.</summary>
    public static IRuleBuilderOptions<T, string> RequiredText<T>(this IRuleBuilder<T, string> rule, int maxLength) =>
        rule.Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("'{PropertyName}' must not be empty.")
            .MaximumLength(maxLength);
}
