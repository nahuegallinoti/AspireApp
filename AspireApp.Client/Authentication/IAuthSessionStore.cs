namespace AspireApp.Client.Authentication;

/// <summary>
/// Persists per-session JWT/refresh tokens server-side, keyed by a sessionId carried in the auth cookie.
/// </summary>
public interface IAuthSessionStore
{
    Task<AuthSession?> GetAsync(string sessionId, CancellationToken ct);

    Task SetAsync(string sessionId, AuthSession session, CancellationToken ct);

    Task RemoveAsync(string sessionId, CancellationToken ct);
}
