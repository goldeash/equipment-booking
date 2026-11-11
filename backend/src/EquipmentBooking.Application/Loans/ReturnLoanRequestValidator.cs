using EquipmentBooking.Domain.Catalogue;
using FluentValidation;

namespace EquipmentBooking.Application.Loans;

public sealed class ReturnLoanRequestValidator : AbstractValidator<ReturnLoanRequest>
{
    public ReturnLoanRequestValidator()
    {
        RuleFor(request => request.ConditionCode).Must(code => ItemConditionCodes.All.Contains(code))
            .WithMessage($"'Condition Code' must be one of: {string.Join(", ", ItemConditionCodes.All)}.");
        RuleFor(request => request.Comment).NotEmpty().When(request => request.ConditionCode == ItemConditionCodes.Damaged)
            .WithMessage("A comment describing the damage is required.");
        RuleFor(request => request.Comment).MaximumLength(1000);
    }
}
