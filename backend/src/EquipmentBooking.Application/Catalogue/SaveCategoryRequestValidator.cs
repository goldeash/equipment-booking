using FluentValidation;

namespace EquipmentBooking.Application.Catalogue;

public sealed class SaveCategoryRequestValidator : AbstractValidator<SaveCategoryRequest>
{
    public SaveCategoryRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Description).MaximumLength(500);
        RuleFor(request => request.MaxLoanDays).InclusiveBetween(1, 365);
        RuleFor(request => request.UsageGuidelines).NotEmpty().MaximumLength(2000);
    }
}
