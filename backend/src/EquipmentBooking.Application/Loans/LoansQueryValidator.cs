using EquipmentBooking.Application.Common.Paging;
using FluentValidation;

namespace EquipmentBooking.Application.Loans;

public sealed class LoansQueryValidator : AbstractValidator<LoansQuery>
{
    public LoansQueryValidator()
    {
        RuleFor(query => query.State).Must(state => LoanStates.All.Contains(state))
            .WithMessage($"'State' must be one of: {string.Join(", ", LoanStates.All)}.");
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PageQuery.MaxPageSize);
    }
}
