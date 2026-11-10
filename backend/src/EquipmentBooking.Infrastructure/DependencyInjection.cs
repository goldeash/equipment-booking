using EquipmentBooking.Application.Catalogue;
using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Notifications;
using EquipmentBooking.Infrastructure.Ai;
using EquipmentBooking.Infrastructure.Jobs;
using EquipmentBooking.Infrastructure.Persistence;
using EquipmentBooking.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBooking.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Default";

    /// <summary>Registers persistence, security services and external adapters.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"The connection string '{ConnectionStringName}' is not configured.");

        services.AddDbContext<AppDbContext>(options => PersistenceOptions.Configure(options, connectionString));
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddSingleton<IEquipmentTextSearch, EquipmentTextSearch>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IPasswordHashing, PasswordHashing>();
        services.AddSingleton<IPurposeAssessor, UnavailablePurposeAssessor>();
        services.AddSingleton(TimeProvider.System);

        services.Configure<NotificationOptions>(configuration.GetSection(NotificationOptions.SectionName));
        services.Configure<JobOptions>(configuration.GetSection(JobOptions.SectionName));
        services.AddSingleton<JobRunner>();
        services.AddHostedService<DailyJobScheduler>();
        return services;
    }
}
