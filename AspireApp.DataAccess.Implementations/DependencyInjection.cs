using AspireApp.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.DataAccess.Implementations;

public static class DependencyInjection
{
    /// <summary>
    /// Registers EF Core <see cref="AppDbContext"/> (InMemory by default) and DA implementations.
    /// Replace <c>UseInMemoryDatabase</c> with <c>UseSqlServer</c>/<c>UseNpgsql</c>/etc. when adopting a real DB.
    /// </summary>
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string inMemoryDatabaseName = "AspireAppDb")
    {
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(inMemoryDatabaseName));

        services.AddScoped<IUserDA, UserDA>();
        services.AddScoped<IProductDA, ProductDA>();
        services.AddScoped<IShowDA, ShowDA>();

        return services;
    }
}
