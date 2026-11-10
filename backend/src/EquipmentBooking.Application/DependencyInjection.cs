using EquipmentBooking.Application.Approval;
using EquipmentBooking.Application.Auth;
using EquipmentBooking.Application.Bookings;
using EquipmentBooking.Application.Catalogue;
using EquipmentBooking.Application.Common.Time;
using EquipmentBooking.Application.Loans;
using EquipmentBooking.Application.Maintenance;
using EquipmentBooking.Application.Notifications;
using EquipmentBooking.Application.Settings;
using EquipmentBooking.Application.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBooking.Application;

public static class DependencyInjection
{
    /// <summary>Registers use case services, approval policy checks and request validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Enabled = false;
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.AddScoped<OrganisationCalendar>();
        services.AddScoped<AuthService>();
        services.AddScoped<EquipmentQueries>();
        services.AddScoped<CatalogueAdminService>();
        services.AddScoped<ReferenceDataService>();
        services.AddScoped<SettingsService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<BookingReader>();
        services.AddScoped<BookingService>();
        services.AddScoped<ApprovalRouter>();
        services.AddScoped<ApprovalQueueService>();
        services.AddScoped<AssessmentLogQueries>();
        services.AddScoped<LoanService>();
        services.AddScoped<LoanMonitoringService>();
        services.AddScoped<MaintenanceService>();
        services.AddScoped<UserAdminService>();
        services.AddScoped<RetentionService>();

        services.AddSingleton<IApprovalPolicyCheck, OrganisationAutoApprovalCheck>();
        services.AddSingleton<IApprovalPolicyCheck, CategoryAutoApprovalCheck>();
        services.AddSingleton<IApprovalPolicyCheck, LowValueCheck>();
        services.AddSingleton<IApprovalPolicyCheck, NotSpecialisedCheck>();
        services.AddSingleton<IApprovalPolicyCheck, WithinMaxDurationCheck>();
        services.AddSingleton<ApprovalPolicyEvaluator>();
        return services;
    }
}
