using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Common;

public static class AuthExtensions
{
    public static IServiceCollection AddKeyClockAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddKeycloakJwtBearer(serviceName: "keycloak", realm: "overflow", options =>
            {
                options.RequireHttpsMetadata = false;
                options.Audience = "overflow";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Default is a 5 minute grace period, which would keep expired tokens working
                    // long after Keycloak considers them dead.
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuers =
                    [
                        "http://localhost:6001/realms/overflow",      // From host machine (Postman)
                        "http://keycloak:8080/realms/overflow",       // Internal Docker network
                        "http://id.overflow.local/realms/overflow"
                    ]
                };
            });

        services.AddAuthorizationBuilder();
        return services;
    }
}