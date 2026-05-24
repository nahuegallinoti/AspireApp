using Microsoft.Extensions.Caching.Hybrid;

namespace AspireApp.Api.Infrastructure;

public static class CachingExtensions
{
    public static WebApplicationBuilder AddCaching(this WebApplicationBuilder builder)
    {
        builder.AddRedisDistributedCache("redis");

        builder.Services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(1)
            };
        });

        return builder;
    }
}
