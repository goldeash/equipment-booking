using EquipmentBooking.Domain.Catalogue;
using FluentValidation;

namespace EquipmentBooking.Application.Catalogue;

public sealed class SaveEquipmentRequestValidator : AbstractValidator<SaveEquipmentRequest>
{
    public SaveEquipmentRequestValidator()
    {
        RuleFor(request => request.InventoryNumber).NotEmpty().MaximumLength(30).Matches("^[A-Za-z0-9-]+$")
            .WithMessage("'Inventory Number' may contain letters, digits and hyphens only.");
        RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Description).MaximumLength(2000);
        RuleFor(request => request.CategoryId).GreaterThan(0);
        RuleFor(request => request.LocationId).GreaterThan(0);
        RuleFor(request => request.Value).InclusiveBetween(0m, 9_999_999_999.99m).PrecisionScale(12, 2, ignoreTrailingZeros: true);
        RuleFor(request => request.ConditionCode).Must(code => ItemConditionCodes.All.Contains(code))
            .WithMessage($"'Condition Code' must be one of: {string.Join(", ", ItemConditionCodes.All)}.");
        RuleFor(request => request.FeatureIds).Must(ids => ids is null || ids.Length <= 20)
            .WithMessage("At most 20 features can be assigned.");
    }
}
