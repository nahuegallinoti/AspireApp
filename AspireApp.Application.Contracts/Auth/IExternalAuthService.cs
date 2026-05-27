using AspireApp.Application.Models.Auth;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Contracts.Auth;

public interface IExternalAuthService
{
    /// <summary>
    /// Validates the supplied id_token (or equivalent) with the SSO provider, links/creates a local user
    /// and emits the standard access+refresh tokens.
    /// </summary>
    Task<Result<AuthenticationResult>> LoginAsync(ExternalLoginRequest request, string? ip, CancellationToken ct);
}

public interface IExternalIdentityValidator
{
    string Provider { get; }

    Task<Result<ExternalIdentity>> ValidateAsync(string idToken, CancellationToken ct);
}

public sealed record ExternalIdentity(
    string Provider,
    string Subject,
    string Email,
    bool EmailVerified,
    string? GivenName,
    string? FamilyName);
