namespace AspireApp.Application.Models.Auth;

public static class AuthPolicies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string AuthenticatedUser = nameof(AuthenticatedUser);
}
