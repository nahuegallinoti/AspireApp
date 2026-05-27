using UserEntity = AspireApp.Domain.Entities.User;

namespace AspireApp.Application.Contracts.Auth;

public interface IAuthTokenService
{
    (string Token, DateTimeOffset ExpiresUtc) CreateAccessToken(UserEntity user, IEnumerable<string> roles);

    (string Token, string TokenHash, DateTimeOffset ExpiresUtc) CreateRefreshToken();

    string HashRefreshToken(string token);
}
