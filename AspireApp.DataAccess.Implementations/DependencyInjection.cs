using AspireApp.Application.Persistence;
using AspireApp.DataAccess.Implementations.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspireApp.DataAccess.Implementations;

public static class DependencyInjection
{
    /// <summary>
    /// Registers EF Core <see cref="AppDbContext"/> (InMemory by default) and DA implementations.
    /// Replace <c>UseInMemoryDatabase</c> with <c>UseSqlServer</c>/<c>UseNpgsql</c>/etc. when adopting a real DB.
    /// </summary>
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string inMemoryDatabaseName = "AspireAppDb")
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddDbContext<AppDbContext>((serviceProvider, options) => options
            .UseInMemoryDatabase(inMemoryDatabaseName)
            .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>()));

        services.AddScoped<IUserDA, UserDA>();
        services.AddScoped<IRoleDA, RoleDA>();
        services.AddScoped<IRefreshTokenDA, RefreshTokenDA>();
        services.AddScoped<IProductDA, ProductDA>();
        services.AddScoped<IShowDA, ShowDA>();

        services.AddScoped<DbSeeder>();

        return services;
    }
}
