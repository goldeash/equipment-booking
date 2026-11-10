using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Domain.Users;
using EquipmentBooking.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EquipmentBooking.Api.Common.Security;

public static class SecurityServiceCollectionExtensions
{
    /// <summary>JWT bearer authentication and role policies; authentication is required by default (NFR-04).</summary>
    public static IServiceCollection AddApiSecurity(this IServiceCollection services, IConfiguration configuration, bool isOpenApiGeneration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = isOpenApiGeneration || string.IsNullOrEmpty(jwt.SigningKey)
                        ? null
                        : TokenService.CreateSigningKey(jwt.SigningKey),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = AppClaimTypes.Role,
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(AuthorizationPolicies.Manager, policy => policy.RequireRole(RoleCodes.Manager))
            .AddPolicy(AuthorizationPolicies.Administrator, policy => policy.RequireRole(RoleCodes.Administrator));
        return services;
    }
}
