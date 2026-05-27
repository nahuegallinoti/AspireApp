using System.Security.Claims;
using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.Client.Authentication;

public static class AuthExtensions
{
    public static IServiceCollection AddAspireAppAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

#pragma warning disable EXTEXP0018
        services.AddHybridCache();
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
        });

        if (googleOpts.Enabled && !string.IsNullOrWhiteSpace(googleOpts.ClientId) && !string.IsNullOrWhiteSpace(googleOpts.ClientSecret))
        {
            authBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = googleOpts.ClientId!;
                options.ClientSecret = googleOpts.ClientSecret!;
                options.CallbackPath = "/signin-google";
                options.SaveTokens = true;
            });
        }

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.AdminOnly, p => p.RequireAuthenticatedUser().RequireRole(RoleNames.Admin))
            .AddPolicy(AuthPolicies.AuthenticatedUser, p => p.RequireAuthenticatedUser());

        services.AddCascadingAuthenticationState();

        return services;
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
