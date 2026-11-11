using FluentValidation;

namespace EquipmentBooking.Application.Catalogue;

public sealed class CreateNamedItemRequestValidator : AbstractValidator<CreateNamedItemRequest>
{
    public CreateNamedItemRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
    }
}
