using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Time;
using EquipmentBooking.Application.Common.Validation;
using EquipmentBooking.Domain.Settings;
using FluentValidation;

namespace EquipmentBooking.Application.Settings;

/// <summary>Organisation settings: low value threshold, automatic approval switch, retention and return time (US-02, US-27).</summary>
public sealed class SettingsService
{
    private readonly IAppDbContext _db;
    private readonly OrganisationCalendar _calendar;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<UpdateApprovalSettingsRequest> _validator;

    public SettingsService(IAppDbContext db, OrganisationCalendar calendar, ICurrentUser currentUser, IValidator<UpdateApprovalSettingsRequest> validator)
    {
        _db = db;
        _calendar = calendar;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<ApprovalSettingsResponse> GetAsync(CancellationToken cancellationToken) =>
        ToResponse(await _calendar.GetSettingsAsync(cancellationToken));

    public async Task<ApprovalSettingsResponse> UpdateAsync(UpdateApprovalSettingsRequest request, CancellationToken cancellationToken)
    {
        await _validator.EnsureValidAsync(request, cancellationToken);
        var settings = await _calendar.GetSettingsAsync(cancellationToken);
        settings.Update(request.LowValueThreshold, request.AutoApprovalEnabled, request.RetentionDays, request.ReturnTime, _currentUser.UserId);
        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(settings);
    }

    private static ApprovalSettingsResponse ToResponse(OrganisationSettings settings) => new(
        settings.LowValueThreshold,
        settings.AutoApprovalEnabled,
        settings.RetentionDays,
        settings.ReturnTime,
        settings.TimeZone);
}
