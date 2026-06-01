using AspireApp.Application.Contracts.Auth;
using AspireApp.Domain.Entities;
using AspireApp.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AspireApp.Tests.Application.Auth;

public class AuthTokenServiceTests
{
    private static readonly JwtOptions JwtOpts = new()
    {
        Key = "ThisIsAVeryLongTestKeyForUnitTestsOnly123!!",
        Issuer = "AspireApp",
        Audience = "AspireApp.Clients",
        AccessTokenLifetimeMinutes = 5,
        RefreshTokenLifetimeDays = 1
    };

    private static AuthTokenService Sut(TimeProvider? time = null) =>
        new(Options.Create(JwtOpts), time ?? TimeProvider.System);

    [Fact]
    public void CreateAccessTokenIncludesUserAndRoleClaims()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Nahuel",
            Surname = "Gallinoti"
        };

        var (token, _) = Sut().CreateAccessToken(user, ["Admin", "User"]);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "sub" && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == user.Email);
        jwt.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)
            .Should().BeEquivalentTo(["Admin", "User"]);
    }

    [Fact]
    public void CreateAccessTokenSignsWithJwtKey()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "u@e.com", Name = "N", Surname = "G" };

        var (token, _) = Sut().CreateAccessToken(user, []);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtOpts.Key)),
            ValidIssuer = JwtOpts.Issuer,
            ValidAudience = JwtOpts.Audience,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false
        };

        new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
    }

    [Fact]
    public void CreateRefreshTokenReturnsRandomTokenAndStableHash()
    {
        var sut = Sut();
        var a = sut.CreateRefreshToken();
        var b = sut.CreateRefreshToken();

        a.Token.Should().NotBe(b.Token);
        a.TokenHash.Should().NotBe(b.TokenHash);

        sut.HashRefreshToken(a.Token).Should().Be(a.TokenHash);
    }
}
