using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.Application.Mappers;

public static class DependencyInjection
{
    public static IServiceCollection AddMappers(this IServiceCollection services)
    {
        services.AddSingleton<UserMapper>();
        services.AddSingleton<RoleMapper>();
        services.AddSingleton<ProductMapper>();
        services.AddSingleton<ShowMapper>();
        return services;
    }
}
