using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AspireApp.Application.Contracts.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserEntity = AspireApp.Domain.Entities.User;

namespace AspireApp.Application.Implementations.Auth;

internal sealed class AuthTokenService(IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider) : IAuthTokenService
{
    private const int RefreshTokenBytes = 64;
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public (string Token, DateTimeOffset ExpiresUtc) CreateAccessToken(UserEntity user, IEnumerable<string> roles)
    {
        var now = timeProvider.GetUtcNow();
        var expires = now.AddMinutes(_jwt.AccessTokenLifetimeMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.Name),
            new(JwtRegisteredClaimNames.FamilyName, user.Surname),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        claims.AddRange(roles.Where(r => !string.IsNullOrWhiteSpace(r))
                             .Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public (string Token, string TokenHash, DateTimeOffset ExpiresUtc) CreateRefreshToken()
    {
        var rawBytes = RandomNumberGenerator.GetBytes(RefreshTokenBytes);
        var token = Convert.ToBase64String(rawBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var hash = HashRefreshToken(token);
        var expires = timeProvider.GetUtcNow().AddDays(_jwt.RefreshTokenLifetimeDays);
        return (token, hash, expires);
    }

    public string HashRefreshToken(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
