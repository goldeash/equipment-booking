using EquipmentBooking.Application.Auth;
using EquipmentBooking.Application.Catalogue;
using EquipmentBooking.Application.Common.Time;
using EquipmentBooking.Application.Settings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBooking.Application;

public static class DependencyInjection
{
    /// <summary>Registers use case services and request validators.</summary>
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
        return services;
    }
}
