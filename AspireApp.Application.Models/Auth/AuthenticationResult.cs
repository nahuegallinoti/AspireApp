namespace AspireApp.Application.Models.Auth;

public sealed record AuthenticationResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresUtc,
    DateTimeOffset RefreshTokenExpiresUtc,
    string TokenType = "Bearer");
