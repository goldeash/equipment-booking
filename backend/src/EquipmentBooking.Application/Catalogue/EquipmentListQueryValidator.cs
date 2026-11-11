using EquipmentBooking.Application.Common.Paging;
using FluentValidation;

namespace EquipmentBooking.Application.Catalogue;

public sealed class EquipmentListQueryValidator : AbstractValidator<EquipmentListQuery>
{
    public static readonly IReadOnlyList<string> SortFields = ["name", "-name", "inventoryNumber", "-inventoryNumber", "value", "-value"];

    public EquipmentListQueryValidator()
    {
        RuleFor(query => query.Q).MaximumLength(100);
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PageQuery.MaxPageSize);
        RuleFor(query => query.Sort).Must(sort => sort is null || SortFields.Contains(sort))
            .WithMessage($"'Sort' must be one of: {string.Join(", ", SortFields)}.");
        RuleFor(query => query)
            .Must(query => query.AvailableFrom.HasValue == query.AvailableTo.HasValue)
            .WithName("availableTo")
            .WithMessage("Both 'availableFrom' and 'availableTo' must be given.");
        RuleFor(query => query.AvailableTo)
            .GreaterThanOrEqualTo(query => query.AvailableFrom)
            .When(query => query.AvailableFrom.HasValue && query.AvailableTo.HasValue)
            .WithMessage("'availableTo' must not be before 'availableFrom'.");
    }
}
