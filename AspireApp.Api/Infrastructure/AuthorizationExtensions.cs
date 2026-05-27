using AspireApp.Application.Models.Auth;

namespace AspireApp.Api.Infrastructure;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.AdminOnly, p => p.RequireAuthenticatedUser().RequireRole(RoleNames.Admin))
            .AddPolicy(AuthPolicies.AuthenticatedUser, p => p.RequireAuthenticatedUser());

        return services;
    }
}
