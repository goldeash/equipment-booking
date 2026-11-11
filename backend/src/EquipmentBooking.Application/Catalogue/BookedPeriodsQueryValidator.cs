using FluentValidation;

namespace EquipmentBooking.Application.Catalogue;

public sealed class BookedPeriodsQueryValidator : AbstractValidator<BookedPeriodsQuery>
{
    public const int MaxRangeDays = 366;

    public BookedPeriodsQueryValidator()
    {
        RuleFor(query => query.To).GreaterThanOrEqualTo(query => query.From);
        RuleFor(query => query)
            .Must(query => query.To.DayNumber - query.From.DayNumber <= MaxRangeDays)
            .WithName("to")
            .WithMessage($"The range must not exceed {MaxRangeDays} days.");
    }
}
