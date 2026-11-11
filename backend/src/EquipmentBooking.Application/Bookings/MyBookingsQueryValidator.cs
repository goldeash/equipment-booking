using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Domain.Bookings;
using FluentValidation;

namespace EquipmentBooking.Application.Bookings;

public sealed class MyBookingsQueryValidator : AbstractValidator<MyBookingsQuery>
{
    public MyBookingsQueryValidator()
    {
        RuleFor(query => query.Status).Must(status => status is null || BookingStatusCodes.All.Contains(status))
            .WithMessage($"'Status' must be one of: {string.Join(", ", BookingStatusCodes.All)}.");
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PageQuery.MaxPageSize);
    }
}
