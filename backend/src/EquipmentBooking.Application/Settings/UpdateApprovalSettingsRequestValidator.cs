using EquipmentBooking.Domain.Settings;
using FluentValidation;

namespace EquipmentBooking.Application.Settings;

public sealed class UpdateApprovalSettingsRequestValidator : AbstractValidator<UpdateApprovalSettingsRequest>
{
    public UpdateApprovalSettingsRequestValidator()
    {
        RuleFor(request => request.LowValueThreshold).GreaterThan(0m).LessThan(10_000_000_000m).PrecisionScale(12, 2, ignoreTrailingZeros: true);
        RuleFor(request => request.RetentionDays).InclusiveBetween(OrganisationSettings.MinRetentionDays, 3650);
    }
}
