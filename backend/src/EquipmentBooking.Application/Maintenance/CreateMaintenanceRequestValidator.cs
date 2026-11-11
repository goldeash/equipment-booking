using FluentValidation;

namespace EquipmentBooking.Application.Maintenance;

public sealed class CreateMaintenanceRequestValidator : AbstractValidator<CreateMaintenanceRequest>
{
    public CreateMaintenanceRequestValidator()
    {
        RuleFor(request => request.MaintenanceTypeCode).NotEmpty().MaximumLength(40);
        RuleFor(request => request.EndDate).GreaterThanOrEqualTo(request => request.StartDate).When(request => request.EndDate.HasValue)
            .WithMessage("The end date must not be before the start date.");
        RuleFor(request => request.Description).NotEmpty().MaximumLength(2000);
    }
}
