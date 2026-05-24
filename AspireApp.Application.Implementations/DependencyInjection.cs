using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Contracts.Product;
using AspireApp.Application.Contracts.Show;
using AspireApp.Application.Contracts.User;
using AspireApp.Application.Implementations.Auth;
using AspireApp.Application.Implementations.Product;
using AspireApp.Application.Implementations.Show;
using AspireApp.Application.Implementations.User;
using AspireApp.Application.Mappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.Application.Implementations;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMappers();

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IRegisterUserService, RegisterUserService>();
        services.AddScoped<IRegisterUserServiceDependencies, RegisterUserServiceDependencies>();

        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<ILoginServiceDependencies, LoginServiceDependencies>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IShowService, ShowService>();

        return services;
    }
}
