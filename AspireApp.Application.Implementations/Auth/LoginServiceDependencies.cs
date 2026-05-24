using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Application.Persistence;
using AspireApp.Domain.ROP;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserEntity = AspireApp.Domain.Entities.User;

namespace AspireApp.Application.Implementations.Auth;

internal sealed class LoginServiceDependencies(
    IOptions<JwtOptions> jwtOptions,
    IUserDA userDA) : ILoginServiceDependencies
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<Result<UserEntity>> VerifyUserPasswordAsync(UserLogin userAccount, CancellationToken ct)
    {
        var user = await userDA.GetByEmailAsync(userAccount.Email, ct);

        if (user is null || !VerifyPasswordHash(userAccount.Password, user.PasswordHash, user.PasswordSalt))
            return Result.Unauthorized<UserEntity>("Invalid credentials.");

        return user;
    }

    public Result<AuthenticationResult> CreateToken(UserEntity user)
    {
        var key = Encoding.UTF8.GetBytes(_jwt.Key);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, $"{user.Name} {user.Surname}".Trim())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(_jwt.TokenLifetimeMinutes),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(handler.CreateToken(descriptor));

        return new AuthenticationResult(token).Success();
    }

    private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        using var hmac = new HMACSHA512(storedSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }
}
