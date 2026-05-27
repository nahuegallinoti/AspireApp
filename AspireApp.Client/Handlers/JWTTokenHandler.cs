using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using AspireApp.Application.Models.Auth;
using AspireApp.Client.ApiClients;
using AspireApp.Client.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspireApp.Client.Handlers;

/// <summary>
/// Attaches the current user's access token to outgoing requests, and transparently rotates
/// access+refresh tokens when the API returns 401.
/// </summary>
public sealed class JwtTokenHandler(
    IHttpContextAccessor httpContextAccessor,
    IAuthSessionStore sessionStore,
    IHttpClientFactory httpClientFactory,
    ILogger<JwtTokenHandler> logger) : DelegatingHandler
{
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var sessionId = GetSessionId();
        AuthSession? session = sessionId is null ? null : await sessionStore.GetAsync(sessionId, ct);

        AttachAccessToken(request, session);

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode != HttpStatusCode.Unauthorized || sessionId is null || session is null)
            return response;

        response.Dispose();

        var refreshed = await TryRefreshAsync(sessionId, session, ct);
        if (refreshed is null)
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);

        var retry = await CloneRequestAsync(request, ct);
        AttachAccessToken(retry, refreshed);
        return await base.SendAsync(retry, ct);
    }

    private string? GetSessionId() =>
        httpContextAccessor.HttpContext?.User?.FindFirstValue(AuthClaimTypes.SessionId);

    private static void AttachAccessToken(HttpRequestMessage request, AuthSession? session)
    {
        if (session is not null && !string.IsNullOrEmpty(session.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        else
            request.Headers.Authorization = null;
    }

    private async Task<AuthSession?> TryRefreshAsync(string sessionId, AuthSession current, CancellationToken ct)
    {
        await RefreshLock.WaitAsync(ct);
        try
        {
            var latest = await sessionStore.GetAsync(sessionId, ct);
            if (latest is not null && latest.AccessToken != current.AccessToken)
                return latest;

            using var refreshClient = httpClientFactory.CreateClient(HttpClientNames.ApiRaw);

            try
            {
                var resp = await refreshClient.PostAsJsonAsync(
                    "api/auth/refresh",
                    new RefreshTokenRequest { RefreshToken = current.RefreshToken },
                    ct);

                if (!resp.IsSuccessStatusCode)
                {
                    logger.LogWarning("Refresh failed with status {Status}.", resp.StatusCode);
                    await sessionStore.RemoveAsync(sessionId, ct);
                    return null;
                }

                var result = await resp.Content.ReadFromJsonAsync<AuthenticationResult>(ct)
                              ?? throw new InvalidOperationException("Empty refresh payload.");

                var rotated = current with
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    AccessTokenExpiresUtc = result.AccessTokenExpiresUtc,
                    RefreshTokenExpiresUtc = result.RefreshTokenExpiresUtc
                };

                await sessionStore.SetAsync(sessionId, rotated, ct);
                return rotated;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Refresh request errored.");
                await sessionStore.RemoveAsync(sessionId, ct);
                return null;
            }
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        if (request.Content is not null)
        {
            var stream = new MemoryStream();
            await request.Content.CopyToAsync(stream, ct);
            stream.Position = 0;
            clone.Content = new StreamContent(stream);
            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }
}
