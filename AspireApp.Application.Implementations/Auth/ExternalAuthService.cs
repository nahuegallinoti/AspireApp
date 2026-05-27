using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Models.Auth;
using AspireApp.Application.Persistence;
using AspireApp.Domain.ROP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserEntity = AspireApp.Domain.Entities.User;
using RefreshTokenEntity = AspireApp.Domain.Entities.RefreshToken;
using UserRoleEntity = AspireApp.Domain.Entities.UserRole;

namespace AspireApp.Application.Implementations.Auth;

internal sealed class ExternalAuthService(
    IUserDA userDA,
    IRoleDA roleDA,
    IRefreshTokenDA refreshTokenDA,
    IAuthTokenService tokenService,
    IEnumerable<IExternalIdentityValidator> validators,
    IOptions<IdentityOptions> identityOptions,
    TimeProvider timeProvider,
    ILogger<ExternalAuthService> logger) : IExternalAuthService
{
    private readonly IdentityOptions _identity = identityOptions.Value;

    public async Task<Result<AuthenticationResult>> LoginAsync(ExternalLoginRequest request, string? ip, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.IdToken))
            return Result.Failure<AuthenticationResult>("Provider and IdToken are required.");

        var validator = validators.FirstOrDefault(v => string.Equals(v.Provider, request.Provider, StringComparison.OrdinalIgnoreCase));
        if (validator is null)
            return Result.Failure<AuthenticationResult>($"External provider '{request.Provider}' is not configured.", System.Net.HttpStatusCode.BadRequest);

        var identityResult = await validator.ValidateAsync(request.IdToken, ct);
        if (identityResult.IsFailure)
            return Result.Failure<AuthenticationResult>(identityResult.Errors, identityResult.HttpStatusCode);

        var identity = identityResult.Value;

        var user = await userDA.GetByExternalAsync(identity.Provider, identity.Subject, ct)
                ?? await userDA.GetByEmailWithRolesAsync(identity.Email, ct);

        if (user is null)
        {
            user = await CreateUserFromExternalAsync(identity, ct);
        }
        else
        {
            if (string.IsNullOrEmpty(user.ExternalProvider))
            {
                user.ExternalProvider = identity.Provider;
                user.ExternalProviderUserId = identity.Subject;
            }
            if (identity.EmailVerified)
                user.EmailConfirmed = true;
            user.LastLoginUtc = timeProvider.GetUtcNow();
            user.AccessFailedCount = 0;
            user.LockoutEndUtc = null;
            userDA.Update(user);
            await userDA.SaveChangesAsync(ct);
        }

        if (!user.IsActive)
            return Result.Unauthorized<AuthenticationResult>("Account is disabled.");

        var roles = user.UserRoles.Select(ur => ur.Role?.Name ?? string.Empty)
                                  .Where(r => !string.IsNullOrWhiteSpace(r));
        var access = tokenService.CreateAccessToken(user, roles);
        var refresh = tokenService.CreateRefreshToken();

        var token = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refresh.TokenHash,
            ExpiresUtc = refresh.ExpiresUtc,
            CreatedUtc = timeProvider.GetUtcNow(),
            CreatedByIp = ip
        };

        await refreshTokenDA.AddAsync(token, ct);
        await refreshTokenDA.SaveChangesAsync(ct);

        logger.LogInformation("External login for {Email} via {Provider}.", identity.Email, identity.Provider);
        return new AuthenticationResult(access.Token, refresh.Token, access.ExpiresUtc, refresh.ExpiresUtc);
    }

    private async Task<UserEntity> CreateUserFromExternalAsync(ExternalIdentity identity, CancellationToken ct)
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = identity.Email,
            NormalizedEmail = identity.Email.ToUpperInvariant(),
            Name = identity.GivenName ?? string.Empty,
            Surname = identity.FamilyName ?? string.Empty,
            EmailConfirmed = identity.EmailVerified,
            IsActive = true,
            ExternalProvider = identity.Provider,
            ExternalProviderUserId = identity.Subject,
            CreatedUtc = timeProvider.GetUtcNow(),
            LastLoginUtc = timeProvider.GetUtcNow()
        };

        var defaultRole = await roleDA.GetByNameAsync(_identity.DefaultRole, ct);
        if (defaultRole is not null)
            user.UserRoles.Add(new UserRoleEntity { UserId = user.Id, RoleId = defaultRole.Id });

        await userDA.AddAsync(user, ct);
        await userDA.SaveChangesAsync(ct);
        return user;
    }
}
