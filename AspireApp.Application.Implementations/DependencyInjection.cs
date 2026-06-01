using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Contracts.Product;
using AspireApp.Application.Contracts.Roles;
using AspireApp.Application.Contracts.Show;
using AspireApp.Application.Contracts.Users;
using AspireApp.Application.Implementations.Auth;
using AspireApp.Application.Implementations.Product;
using AspireApp.Application.Implementations.Roles;
using AspireApp.Application.Implementations.Show;
using AspireApp.Application.Implementations.Users;
using AspireApp.Application.Mappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.Application.Implementations;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMappers();
        services.TryAddTimeProvider();

        services
            .AddOptions<IdentityOptions>()
            .Bind(configuration.GetSection(IdentityOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IExternalAuthService, ExternalAuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IShowService, ShowService>();

        return services;
    }

    private static void TryAddTimeProvider(this IServiceCollection services)
    {
        if (!services.Any(s => s.ServiceType == typeof(TimeProvider)))
            services.AddSingleton(TimeProvider.System);
    }
}
