using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace AspireApp.Client.Authentication;

internal static class GoogleAuthTokenResolver
{
    public static async Task<(string? IdToken, string? AccessToken)> ResolveAsync(HttpContext httpContext)
    {
        var idToken = await httpContext.GetTokenAsync(AuthClaimTypes.CookieScheme, "id_token");
        var accessToken = await httpContext.GetTokenAsync(AuthClaimTypes.CookieScheme, "access_token");

        if (!string.IsNullOrEmpty(idToken) || !string.IsNullOrEmpty(accessToken))
            return (idToken, accessToken);

        var cookie = await httpContext.AuthenticateAsync(AuthClaimTypes.CookieScheme);
        idToken ??= cookie.Properties?.GetTokenValue("id_token");
        accessToken ??= cookie.Properties?.GetTokenValue("access_token");

        if (!string.IsNullOrEmpty(idToken) || !string.IsNullOrEmpty(accessToken))
            return (idToken, accessToken);

        var google = await httpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        idToken ??= google.Properties?.GetTokenValue("id_token");
        accessToken ??= google.Properties?.GetTokenValue("access_token");

        return (idToken, accessToken);
    }

    /// <summary>
    /// Persists id_token from the raw OAuth token response when the handler did not store it.
    /// </summary>
    public static void PersistIdTokenFromOAuthResponse(OAuthCreatingTicketContext context)
    {
        if (context.TokenResponse?.Response is null)
            return;

        try
        {
            if (!context.TokenResponse.Response.RootElement.TryGetProperty("id_token", out var idTokenElement))
                return;

            var idToken = idTokenElement.GetString();
            if (string.IsNullOrEmpty(idToken))
                return;

            var tokens = context.Properties?.GetTokens().ToList() ?? [];
            tokens.RemoveAll(t => t.Name == "id_token");
            tokens.Add(new AuthenticationToken { Name = "id_token", Value = idToken });
            context.Properties?.StoreTokens(tokens);
        }
        catch (JsonException)
        {
            // Ignore malformed token payloads.
        }
    }
}
