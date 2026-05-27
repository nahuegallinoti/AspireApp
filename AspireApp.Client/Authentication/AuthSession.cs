namespace AspireApp.Client.Authentication;

/// <summary>
/// In-memory representation of the authenticated session. Kept server-side only — never sent to the browser.
/// </summary>
public sealed record AuthSession(
    Guid UserId,
    string Email,
    string Name,
    string Surname,
    IReadOnlyList<string> Roles,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresUtc,
    DateTimeOffset RefreshTokenExpiresUtc);
