using System.Net;
using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Implementations.Auth;
using AspireApp.Application.Mappers;
using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.DataAccess.Implementations;
using AspireApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AspireApp.Tests.Application.Auth;

public class AuthServiceTests
{
    private static (AuthService sut, AppDbContext ctx, IPasswordHasher hasher, IAuthTokenService tokenSvc) BuildSut(IdentityOptions? identity = null)
    {
        var ctx = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"auth-{Guid.NewGuid():N}")
            .Options);

        identity ??= new IdentityOptions { MaxFailedAccessAttempts = 3, LockoutMinutes = 10, PasswordIterations = 10_000, DefaultRole = "User" };
        var identityOptions = Options.Create(identity);
        var jwtOptions = Options.Create(new JwtOptions
        {
            Key = "ThisIsAVeryLongTestKeyForUnitTestsOnly123!!",
            Issuer = "AspireApp",
            Audience = "AspireApp.Clients",
            AccessTokenLifetimeMinutes = 5,
            RefreshTokenLifetimeDays = 1
        });

        var time = TimeProvider.System;
        var hasher = new Pbkdf2PasswordHasher(identityOptions);
        var tokenSvc = new AuthTokenService(jwtOptions, time);

        var sut = new AuthService(
            new UserDA(ctx),
            new RoleDA(ctx),
            new RefreshTokenDA(ctx),
            tokenSvc,
            hasher,
            new UserMapper(),
            identityOptions,
            time,
            NullLogger<AuthService>.Instance);

        return (sut, ctx, hasher, tokenSvc);
    }

    private static async Task<User> SeedUserAsync(AppDbContext ctx, IPasswordHasher hasher, string email = "user@example.com", string password = "Strong!Password1", bool isActive = true)
    {
        var (hash, salt, iterations) = hasher.Hash(password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            Name = "N",
            Surname = "G",
            IsActive = isActive,
            PasswordHash = hash,
            PasswordSalt = salt,
            PasswordIterations = iterations
        };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task LoginReturnsTokensWhenCredentialsAreValid()
    {
        var (sut, ctx, hasher, _) = BuildSut();
        await SeedUserAsync(ctx, hasher);

        var result = await sut.LoginAsync(new UserLogin { Email = "user@example.com", Password = "Strong!Password1" }, ip: "127.0.0.1", CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Value.AccessToken.Should().NotBeEmpty();
        result.Value.RefreshToken.Should().NotBeEmpty();
        result.Value.AccessTokenExpiresUtc.Should().BeAfter(DateTimeOffset.UtcNow);
        ctx.RefreshTokens.Should().HaveCount(1);
    }

    [Fact]
    public async Task LoginReturnsUnauthorizedWhenPasswordIsWrong()
    {
        var (sut, ctx, hasher, _) = BuildSut();
        await SeedUserAsync(ctx, hasher);

        var result = await sut.LoginAsync(new UserLogin { Email = "user@example.com", Password = "WrongPwd1!" }, ip: null, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.HttpStatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginLocksOutAfterTooManyFailedAttempts()
    {
        var identity = new IdentityOptions { MaxFailedAccessAttempts = 2, LockoutMinutes = 5, PasswordIterations = 10_000, DefaultRole = "User" };
        var (sut, ctx, hasher, _) = BuildSut(identity);
        await SeedUserAsync(ctx, hasher);

        await sut.LoginAsync(new UserLogin { Email = "user@example.com", Password = "WrongPwd1!" }, null, CancellationToken.None);
        await sut.LoginAsync(new UserLogin { Email = "user@example.com", Password = "WrongPwd1!" }, null, CancellationToken.None);

        var locked = await sut.LoginAsync(new UserLogin { Email = "user@example.com", Password = "Strong!Password1" }, null, CancellationToken.None);

        locked.Success.Should().BeFalse();
        locked.HttpStatusCode.Should().Be(HttpStatusCode.Locked);
    }

    [Fact]
    public async Task RefreshRotatesRefreshTokenAndInvalidatesOldOne()
    {
        var (sut, ctx, hasher, tokenSvc) = BuildSut();
        await SeedUserAsync(ctx, hasher);

        var login = await sut.LoginAsync(new UserLogin { Email = "user@example.com", Password = "Strong!Password1" }, null, CancellationToken.None);
        var firstRefresh = login.Value.RefreshToken;

        var refreshed = await sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = firstRefresh }, null, CancellationToken.None);

        refreshed.Success.Should().BeTrue();
        refreshed.Value.RefreshToken.Should().NotBe(firstRefresh);

        var reuse = await sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = firstRefresh }, null, CancellationToken.None);
        reuse.Success.Should().BeFalse();
        reuse.HttpStatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LogoutRevokesRefreshToken()
    {
        var (sut, ctx, hasher, _) = BuildSut();
        await SeedUserAsync(ctx, hasher);

        var login = await sut.LoginAsync(new UserLogin { Email = "user@example.com", Password = "Strong!Password1" }, null, CancellationToken.None);

        var logout = await sut.LogoutAsync(new LogoutRequest { RefreshToken = login.Value.RefreshToken }, null, CancellationToken.None);
        logout.Success.Should().BeTrue();

        var afterLogout = await sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = login.Value.RefreshToken }, null, CancellationToken.None);
        afterLogout.Success.Should().BeFalse();
    }

    [Fact]
    public async Task LoginReturnsUnauthorizedForDisabledAccount()
    {
        var (sut, ctx, hasher, _) = BuildSut();
        await SeedUserAsync(ctx, hasher, isActive: false);

        var result = await sut.LoginAsync(new UserLogin { Email = "user@example.com", Password = "Strong!Password1" }, null, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.HttpStatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
