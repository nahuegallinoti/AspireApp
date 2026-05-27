using System.Security.Claims;
using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspireApp.Client.Authentication;

public static class AuthExtensions
{
    public static IServiceCollection AddAspireAppAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

#pragma warning disable EXTEXP0018
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(30),
                LocalCacheExpiration = TimeSpan.FromMinutes(10)
            };
        });
#pragma warning restore EXTEXP0018

        services.AddSingleton<IAuthSessionStore, AuthSessionStore>();

        services
            .AddOptions<GoogleClientOptions>()
            .Bind(configuration.GetSection(GoogleClientOptions.SectionName));

        var googleOpts = configuration.GetSection(GoogleClientOptions.SectionName).Get<GoogleClientOptions>() ?? new();

        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = AuthClaimTypes.CookieScheme;
            options.DefaultSignInScheme = AuthClaimTypes.CookieScheme;
            options.DefaultChallengeScheme = AuthClaimTypes.CookieScheme;
        })
        .AddCookie(AuthClaimTypes.CookieScheme, options =>
        {
            options.Cookie.Name = "AspireApp.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";

            // The cookie carries identity claims, but the access/refresh tokens live in
            // the IAuthSessionStore. If the store loses the session (e.g. process restart
            // with no L2 cache, or session was explicitly revoked) we must reject the
            // cookie so the UI doesn't show a logged-in state while every API call fails
            // with "invalid credentials".
            options.Events.OnValidatePrincipal = ValidateSessionExistsAsync;
        });

        if (googleOpts.Enabled && !string.IsNullOrWhiteSpace(googleOpts.ClientId) && !string.IsNullOrWhiteSpace(googleOpts.ClientSecret))
        {
            authBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = AuthClaimTypes.CookieScheme;
                options.ClientId = googleOpts.ClientId!;
                options.ClientSecret = googleOpts.ClientSecret!;
                options.CallbackPath = "/signin-google";
                options.SaveTokens = true;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Events.OnCreatingTicket = context =>
                {
                    GoogleAuthTokenResolver.PersistIdTokenFromOAuthResponse(context);
                    return Task.CompletedTask;
                };
            });
        }

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.AdminOnly, p => p.RequireAuthenticatedUser().RequireRole(RoleNames.Admin))
            .AddPolicy(AuthPolicies.AuthenticatedUser, p => p.RequireAuthenticatedUser());

        services.AddCascadingAuthenticationState();

        return services;
    }

    private static async Task ValidateSessionExistsAsync(CookieValidatePrincipalContext context)
    {
        var sid = context.Principal?.FindFirstValue(AuthClaimTypes.SessionId);
        if (string.IsNullOrEmpty(sid))
        {
            // Google SSO signs into this cookie (with OAuth tokens, no sid yet) before
            // /auth/google/callback exchanges them for an app session. Rejecting here would
            // sign out and wipe id_token/access_token before the callback can read them.
            var path = context.HttpContext.Request.Path;
            if (path.StartsWithSegments("/auth/google/callback", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/signin-google", StringComparison.OrdinalIgnoreCase))
                return;

            var props = context.Properties;
            if (props is not null
                && (!string.IsNullOrEmpty(props.GetTokenValue("id_token"))
                    || !string.IsNullOrEmpty(props.GetTokenValue("access_token"))))
                return;

            await RejectAsync(context);
            return;
        }

        var services = context.HttpContext.RequestServices;
        var store = services.GetRequiredService<IAuthSessionStore>();
        var ct = context.HttpContext.RequestAborted;

        AuthSession? session;
        try
        {
            session = await store.GetAsync(sid, ct);
        }
        catch (Exception ex)
        {
            // Don't lock users out if the session store backend (e.g. Redis) hiccups —
            // the JwtTokenHandler will surface a 401 if the request actually needs a token.
            services.GetService<ILoggerFactory>()?
                .CreateLogger(typeof(AuthExtensions))
                .LogWarning(ex, "Auth session store lookup failed; allowing cookie through.");
            return;
        }

        if (session is null)
            await RejectAsync(context);
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(AuthClaimTypes.CookieScheme);
    }

    /// <summary>
    /// Builds the ClaimsPrincipal stored in the cookie. Only stable identity info goes in claims;
    /// tokens are kept server-side in the <see cref="IAuthSessionStore"/>.
    /// </summary>
    public static ClaimsPrincipal BuildPrincipal(UserDto user, string sessionId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.Name} {user.Surname}".Trim()),
            new(ClaimTypes.GivenName, user.Name),
            new(ClaimTypes.Surname, user.Surname),
            new(AuthClaimTypes.SessionId, sessionId)
        };

        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, AuthClaimTypes.CookieScheme,
            ClaimTypes.Name, ClaimTypes.Role);
        return new ClaimsPrincipal(identity);
    }
}

public sealed class GoogleClientOptions
{
    public const string SectionName = "Sso:Google";

    public bool Enabled { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}
