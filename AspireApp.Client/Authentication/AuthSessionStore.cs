using Microsoft.Extensions.Caching.Hybrid;

namespace AspireApp.Client.Authentication;

internal sealed class AuthSessionStore(HybridCache cache) : IAuthSessionStore
{
    private const string Prefix = "auth:session:";
    private static readonly HybridCacheEntryOptions DefaultOptions = new()
    {
        Expiration = TimeSpan.FromDays(30),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };

    public async Task<AuthSession?> GetAsync(string sessionId, CancellationToken ct) =>
        await cache.GetOrCreateAsync<AuthSession?>(
            Prefix + sessionId,
            _ => ValueTask.FromResult<AuthSession?>(null),
            DefaultOptions,
            cancellationToken: ct);

    public async Task SetAsync(string sessionId, AuthSession session, CancellationToken ct)
    {
        var ttl = session.RefreshTokenExpiresUtc - DateTimeOffset.UtcNow;
        var options = new HybridCacheEntryOptions
        {
            Expiration = ttl > TimeSpan.FromMinutes(1) ? ttl : TimeSpan.FromHours(1),
            LocalCacheExpiration = TimeSpan.FromMinutes(10)
        };
        await cache.SetAsync(Prefix + sessionId, session, options, cancellationToken: ct);
    }

    public Task RemoveAsync(string sessionId, CancellationToken ct) =>
        cache.RemoveAsync(Prefix + sessionId, ct).AsTask();
}
