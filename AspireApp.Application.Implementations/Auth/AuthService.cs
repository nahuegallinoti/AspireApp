using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Implementations.Extensions;
using AspireApp.Application.Mappers;
using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Application.Models.Users;
using AspireApp.Application.Persistence;
using AspireApp.Domain.ROP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserEntity = AspireApp.Domain.Entities.User;
using RefreshTokenEntity = AspireApp.Domain.Entities.RefreshToken;
using UserRoleEntity = AspireApp.Domain.Entities.UserRole;

namespace AspireApp.Application.Implementations.Auth;

internal sealed class AuthService(
    IUserDA userDA,
    IRoleDA roleDA,
    IRefreshTokenDA refreshTokenDA,
    IAuthTokenService tokenService,
    IPasswordHasher passwordHasher,
    UserMapper userMapper,
    IOptions<IdentityOptions> identityOptions,
    TimeProvider timeProvider,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly IdentityOptions _identity = identityOptions.Value;

    public Task<Result<AuthenticationResult>> LoginAsync(UserLogin login, string? ip, CancellationToken ct) =>
        login.Validate()
            .Bind(validated => AuthenticateAsync(validated, ip, ct));

    public Task<Result<AuthenticationResult>> RegisterAsync(UserRegister register, string? ip, CancellationToken ct) =>
        register.Validate()
            .Bind(validated => CreateLocalUserAsync(validated, ip, ct));

    public async Task<Result<AuthenticationResult>> RefreshAsync(RefreshTokenRequest request, string? ip, CancellationToken ct)
    {
        var validation = request.Validate();
        if (validation.IsFailure)
            return Result.Failure<AuthenticationResult>(validation.Errors, validation.HttpStatusCode);

        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var stored = await refreshTokenDA.GetActiveByHashAsync(hash, ct);

        if (stored is null)
        {
            logger.LogWarning("Refresh token reuse or unknown token attempted from {Ip}.", ip);
            return Result.Unauthorized<AuthenticationResult>("Invalid refresh token.");
        }

        if (!stored.IsActive)
            return Result.Unauthorized<AuthenticationResult>("Invalid refresh token.");

        var user = await userDA.GetByIdWithRolesAsync(stored.UserId, ct);
        if (user is null || !user.IsActive || user.IsLockedOut)
            return Result.Unauthorized<AuthenticationResult>("User is not allowed to refresh.");

        var roles = RolesOf(user);
        var access = tokenService.CreateAccessToken(user, roles);
        var refresh = tokenService.CreateRefreshToken();

        stored.RevokedUtc = timeProvider.GetUtcNow();
        stored.RevokedByIp = ip;
        stored.RevokedReason = "Rotated";

        var newToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refresh.TokenHash,
            ExpiresUtc = refresh.ExpiresUtc,
            CreatedUtc = timeProvider.GetUtcNow(),
            CreatedByIp = ip
        };

        await refreshTokenDA.AddAsync(newToken, ct);
        stored.ReplacedByTokenId = newToken.Id;
        refreshTokenDA.Update(stored);

        await refreshTokenDA.SaveChangesAsync(ct);

        return new AuthenticationResult(access.Token, refresh.Token, access.ExpiresUtc, refresh.ExpiresUtc);
    }

    public async Task<Result<Unit>> LogoutAsync(LogoutRequest request, string? ip, CancellationToken ct)
    {
        var validation = request.Validate();
        if (validation.IsFailure)
            return Result.Failure(validation.Errors, validation.HttpStatusCode);

        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var stored = await refreshTokenDA.GetActiveByHashAsync(hash, ct);

        if (stored is null)
            return Result.Success();

        stored.RevokedUtc = timeProvider.GetUtcNow();
        stored.RevokedByIp = ip;
        stored.RevokedReason = "Logout";
        refreshTokenDA.Update(stored);
        await refreshTokenDA.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<UserDto>> GetCurrentAsync(Guid userId, CancellationToken ct)
    {
        var user = await userDA.GetByIdWithRolesAsync(userId, ct);
        return user is null
            ? Result.NotFound<UserDto>("User not found.")
            : UserMapper.ToDto(user).Success();
    }

    private async Task<Result<AuthenticationResult>> AuthenticateAsync(UserLogin login, string? ip, CancellationToken ct)
    {
        var user = await userDA.GetByEmailWithRolesAsync(login.Email, ct);
        if (user is null)
            return Result.Unauthorized<AuthenticationResult>("Invalid credentials.");

        if (!user.IsActive)
            return Result.Unauthorized<AuthenticationResult>("Account is disabled.");

        if (user.IsLockedOut)
            return Result.Failure<AuthenticationResult>("Account is locked. Please try again later.", System.Net.HttpStatusCode.Locked);

        if (!user.HasPassword)
            return Result.Unauthorized<AuthenticationResult>("This account uses single sign-on. Please log in with the configured provider.");

        var ok = passwordHasher.Verify(login.Password, user.PasswordHash!, user.PasswordSalt!, user.PasswordIterations);
        if (!ok)
        {
            await RegisterFailedLoginAsync(user, ct);
            return Result.Unauthorized<AuthenticationResult>("Invalid credentials.");
        }

        user.AccessFailedCount = 0;
        user.LockoutEndUtc = null;
        user.LastLoginUtc = timeProvider.GetUtcNow();
        userDA.Update(user);

        return await IssueTokensAsync(user, ip, ct);
    }

    private async Task<Result<AuthenticationResult>> CreateLocalUserAsync(UserRegister register, string? ip, CancellationToken ct)
    {
        if (register.Password.Length < _identity.MinPasswordLength)
            return Result.Failure<AuthenticationResult>($"Password must be at least {_identity.MinPasswordLength} characters long.");

        if (await userDA.ExistsAsync(register.Email, ct))
            return Result.Conflict<AuthenticationResult>("Email already in use.");

        var entity = userMapper.ToEntity(register);
        var (hash, salt, iterations) = passwordHasher.Hash(register.Password);
        entity.PasswordHash = hash;
        entity.PasswordSalt = salt;
        entity.PasswordIterations = iterations;
        entity.EmailConfirmed = false;
        entity.IsActive = true;
        entity.CreatedUtc = timeProvider.GetUtcNow();

        var defaultRole = await roleDA.GetByNameAsync(_identity.DefaultRole, ct);
        if (defaultRole is not null)
            entity.UserRoles.Add(new UserRoleEntity { UserId = entity.Id, RoleId = defaultRole.Id });

        await userDA.AddAsync(entity, ct);
        await userDA.SaveChangesAsync(ct);

        return await IssueTokensAsync(entity, ip, ct);
    }

    private async Task<Result<AuthenticationResult>> IssueTokensAsync(UserEntity user, string? ip, CancellationToken ct)
    {
        var roles = RolesOf(user);
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

        return new AuthenticationResult(access.Token, refresh.Token, access.ExpiresUtc, refresh.ExpiresUtc);
    }

    private async Task RegisterFailedLoginAsync(UserEntity user, CancellationToken ct)
    {
        user.AccessFailedCount += 1;

        if (_identity.MaxFailedAccessAttempts > 0 && user.AccessFailedCount >= _identity.MaxFailedAccessAttempts)
        {
            user.LockoutEndUtc = timeProvider.GetUtcNow().AddMinutes(_identity.LockoutMinutes);
            user.AccessFailedCount = 0;
            logger.LogWarning("Locked out user {UserId} until {LockoutEnd}.", user.Id, user.LockoutEndUtc);
        }

        userDA.Update(user);
        await userDA.SaveChangesAsync(ct);
    }

    private static IEnumerable<string> RolesOf(UserEntity user) =>
        user.UserRoles?.Select(ur => ur.Role?.Name ?? string.Empty)
                       .Where(r => !string.IsNullOrWhiteSpace(r)) ?? [];
}
