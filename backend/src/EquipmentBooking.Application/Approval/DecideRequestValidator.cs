using EquipmentBooking.Domain.Bookings;
using FluentValidation;

namespace EquipmentBooking.Application.Approval;

public sealed class DecideRequestValidator : AbstractValidator<DecideRequest>
{
    public DecideRequestValidator()
    {
        RuleFor(request => request.Outcome).Must(outcome => outcome is DecisionOutcomes.Approved or DecisionOutcomes.Declined)
            .WithMessage($"'Outcome' must be '{DecisionOutcomes.Approved}' or '{DecisionOutcomes.Declined}'.");
        RuleFor(request => request.Comment).NotEmpty().When(request => request.Outcome == DecisionOutcomes.Declined)
            .WithMessage("A comment is required to decline a request.");
        RuleFor(request => request.Comment).MaximumLength(1000);
    }
}
