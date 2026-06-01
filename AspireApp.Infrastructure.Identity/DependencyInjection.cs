using AspireApp.Application.Contracts.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.Infrastructure.Identity;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the identity adapters (password hashing, JWT issuance and external SSO validation)
    /// together with their bound options. The matching abstractions live in
    /// <c>AspireApp.Application.Contracts.Auth</c>; the application layer depends only on those.
    /// </summary>
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<SsoOptions>()
            .Bind(configuration.GetSection(SsoOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAuthTokenService, AuthTokenService>();

        services.AddHttpClient(nameof(GoogleIdentityValidator));
        services.AddSingleton<IExternalIdentityValidator, GoogleIdentityValidator>();

        return services;
    }
}
