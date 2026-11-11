using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Domain.Approval;
using FluentValidation;

namespace EquipmentBooking.Application.Approval;

public sealed class AssessmentLogQueryValidator : AbstractValidator<AssessmentLogQuery>
{
    public AssessmentLogQueryValidator()
    {
        RuleFor(query => query.Outcome)
            .Must(outcome => outcome is null or AssessmentOutcomes.Pending or AssessmentOutcomes.Approved or AssessmentOutcomes.Routed)
            .WithMessage("'Outcome' must be pending, approved or routed.");
        RuleFor(query => query.To).GreaterThanOrEqualTo(query => query.From).When(query => query.From.HasValue && query.To.HasValue);
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PageQuery.MaxPageSize);
    }
}
