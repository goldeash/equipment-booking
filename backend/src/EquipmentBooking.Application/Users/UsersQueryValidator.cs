using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Domain.Users;
using FluentValidation;

namespace EquipmentBooking.Application.Users;

public sealed class UsersQueryValidator : AbstractValidator<UsersQuery>
{
    public UsersQueryValidator()
    {
        RuleFor(query => query.Q).MaximumLength(100);
        RuleFor(query => query.Role).Must(role => role is null || RoleCodes.All.Contains(role))
            .WithMessage($"'Role' must be one of: {string.Join(", ", RoleCodes.All)}.");
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PageQuery.MaxPageSize);
    }
}
