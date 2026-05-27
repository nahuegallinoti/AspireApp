using System.Security.Claims;
using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Application.Models.Users;
using AspireApp.Client.ApiClients;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;

namespace AspireApp.Client.Authentication;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/auth");

        group.MapPost("/login", LoginAsync).DisableAntiforgery();
        group.MapPost("/register", RegisterAsync).DisableAntiforgery();
        group.MapPost("/logout", LogoutAsync).DisableAntiforgery();

        group.MapGet("/google/challenge", GoogleChallenge);
        group.MapGet("/google/callback", GoogleCallbackAsync);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        HttpContext httpContext,
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] string? returnUrl,
        AuthApiClient authApi,
        IAuthSessionStore sessionStore,
        CancellationToken ct)
    {
        var login = new UserLogin { Email = email, Password = password };
        var result = await authApi.LoginAsync(login, ct);

        if (result.IsFailure)
            return Results.Redirect(BuildLoginUrl(returnUrl, errors: result.Errors));

        var user = ReadUserFromAccessToken(result.Value.AccessToken);
        if (user is null)
            return Results.Redirect(BuildLoginUrl(returnUrl, errors: ["Could not load user profile."]));

        await EstablishSessionAsync(httpContext, sessionStore, user, result.Value, ct);

        return Results.LocalRedirect(SafeReturnUrl(returnUrl));
    }

    private static async Task<IResult> RegisterAsync(
        HttpContext httpContext,
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] string name,
        [FromForm] string surname,
        [FromForm] string? returnUrl,
        AuthApiClient authApi,
        IAuthSessionStore sessionStore,
        CancellationToken ct)
    {
        var register = new UserRegister { Email = email, Password = password, Name = name, Surname = surname };
        var result = await authApi.RegisterAsync(register, ct);

        if (result.IsFailure)
            return Results.Redirect(BuildRegisterUrl(returnUrl, errors: result.Errors));

        var user = ReadUserFromAccessToken(result.Value.AccessToken);
        if (user is null)
            return Results.Redirect(BuildLoginUrl(returnUrl, errors: ["Could not load user profile."]));

        await EstablishSessionAsync(httpContext, sessionStore, user, result.Value, ct);

        return Results.LocalRedirect(SafeReturnUrl(returnUrl));
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext httpContext,
        AuthApiClient authApi,
        IAuthSessionStore sessionStore,
        CancellationToken ct)
    {
        var sid = httpContext.User.FindFirstValue(AuthClaimTypes.SessionId);
        if (!string.IsNullOrEmpty(sid))
        {
            var session = await sessionStore.GetAsync(sid, ct);
            if (session is not null)
            {
                _ = authApi.LogoutAsync(session.RefreshToken, ct);
                await sessionStore.RemoveAsync(sid, ct);
            }
        }

        await httpContext.SignOutAsync(AuthClaimTypes.CookieScheme);
        return Results.LocalRedirect("/");
    }

    private static IResult GoogleChallenge([FromQuery] string? returnUrl, HttpContext httpContext)
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = $"/auth/google/callback?returnUrl={Uri.EscapeDataString(SafeReturnUrl(returnUrl))}"
        };
        return Results.Challenge(props, [GoogleDefaults.AuthenticationScheme]);
    }

    private static async Task<IResult> GoogleCallbackAsync(
        [FromQuery] string? returnUrl,
        HttpContext httpContext,
        AuthApiClient authApi,
        IAuthSessionStore sessionStore,
        CancellationToken ct)
    {
        var (idToken, accessToken) = await GoogleAuthTokenResolver.ResolveAsync(httpContext);
        if (string.IsNullOrEmpty(idToken) && string.IsNullOrEmpty(accessToken))
            return Results.Redirect(BuildLoginUrl(returnUrl, errors: ["Google did not return authentication tokens."]));

        var result = await authApi.LoginWithGoogleAsync(idToken, accessToken, ct);
        if (result.IsFailure)
            return Results.Redirect(BuildLoginUrl(returnUrl, errors: result.Errors));

        var user = ReadUserFromAccessToken(result.Value.AccessToken);
        if (user is null)
            return Results.Redirect(BuildLoginUrl(returnUrl, errors: ["Could not load user profile."]));

        await httpContext.SignOutAsync(AuthClaimTypes.CookieScheme);
        await EstablishSessionAsync(httpContext, sessionStore, user, result.Value, ct);

        return Results.LocalRedirect(SafeReturnUrl(returnUrl));
    }

    /// <summary>
    /// Decodes claims from the just-issued access token. The token was already signed and validated
    /// by the API, so its claims are trustworthy and avoid an extra roundtrip during sign-in.
    /// </summary>
    private static UserDto? ReadUserFromAccessToken(string accessToken)
    {
        try
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);
            var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var given = jwt.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
            var family = jwt.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value;
            var roles = jwt.Claims
                .Where(c => c.Type is "role" or "roles" or ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(email))
                return null;

            return new UserDto(Guid.Parse(sub), email, given ?? string.Empty, family ?? string.Empty,
                IsActive: true, EmailConfirmed: true, ExternalProvider: null,
                CreatedUtc: DateTimeOffset.UtcNow, LastLoginUtc: DateTimeOffset.UtcNow, Roles: roles);
        }
        catch
        {
            return null;
        }
    }

    private static async Task EstablishSessionAsync(
        HttpContext httpContext,
        IAuthSessionStore sessionStore,
        UserDto user,
        AuthenticationResult tokens,
        CancellationToken ct)
    {
        var sessionId = Guid.NewGuid().ToString("N");

        var session = new AuthSession(
            user.Id,
            user.Email,
            user.Name,
            user.Surname,
            user.Roles,
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresUtc,
            tokens.RefreshTokenExpiresUtc);

        await sessionStore.SetAsync(sessionId, session, ct);

        var principal = AuthExtensions.BuildPrincipal(user, sessionId);
        await httpContext.SignInAsync(AuthClaimTypes.CookieScheme, principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = tokens.RefreshTokenExpiresUtc
        });
    }

    private static string BuildLoginUrl(string? returnUrl, IEnumerable<string>? errors = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["returnUrl"] = SafeReturnUrl(returnUrl)
        };
        if (errors is not null)
            query["error"] = string.Join("; ", errors);

        return QueryHelpers.AddQueryString("/login", query);
    }

    private static string BuildRegisterUrl(string? returnUrl, IEnumerable<string>? errors = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["returnUrl"] = SafeReturnUrl(returnUrl)
        };
        if (errors is not null)
            query["error"] = string.Join("; ", errors);

        return QueryHelpers.AddQueryString("/register", query);
    }

    private static string SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl)) return "/";
        if (!returnUrl.StartsWith('/') || returnUrl.StartsWith("//", StringComparison.Ordinal))
            return "/";
        return returnUrl;
    }
}
