using EquipmentBooking.Domain.Bookings;
using FluentValidation;

namespace EquipmentBooking.Application.Bookings;

public sealed class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(request => request.EquipmentItemId).GreaterThan(0);
        RuleFor(request => request.EndDate).GreaterThanOrEqualTo(request => request.StartDate)
            .WithMessage("The end date must not be before the start date.");
        RuleFor(request => request.Purpose).NotEmpty()
            .Length(Booking.MinPurposeLength, Booking.MaxPurposeLength);
    }
}
