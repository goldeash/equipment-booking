using FluentValidation;

namespace EquipmentBooking.Application.Bookings;

public sealed class ChangeBookingDatesRequestValidator : AbstractValidator<ChangeBookingDatesRequest>
{
    public ChangeBookingDatesRequestValidator()
    {
        RuleFor(request => request.EndDate).GreaterThanOrEqualTo(request => request.StartDate)
            .WithMessage("The end date must not be before the start date.");
    }
}
