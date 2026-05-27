using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Application.Models.Users;
using AspireApp.Domain.ROP;

namespace AspireApp.Client.ApiClients;

public sealed class AuthApiClient(IHttpClientFactory httpClientFactory)
{
    private readonly AnonymousApiClient _anonymous = new(httpClientFactory);
    private readonly AuthorizedApiClient _authorized = new(httpClientFactory);

    public Task<Result<AuthenticationResult>> LoginAsync(UserLogin user, CancellationToken ct) =>
        _anonymous.PostAsync<AuthenticationResult, UserLogin>("api/auth/login", user, ct);

    public Task<Result<AuthenticationResult>> RegisterAsync(UserRegister user, CancellationToken ct) =>
        _anonymous.PostAsync<AuthenticationResult, UserRegister>("api/auth/register", user, ct);

    public Task<Result<AuthenticationResult>> RefreshAsync(string refreshToken, CancellationToken ct) =>
        _anonymous.PostAsync<AuthenticationResult, RefreshTokenRequest>("api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = refreshToken }, ct);

    public Task<Result<Unit>> LogoutAsync(string refreshToken, CancellationToken ct) =>
        _anonymous.PostAsync<Unit, LogoutRequest>("api/auth/logout", new LogoutRequest { RefreshToken = refreshToken }, ct);

    public Task<Result<UserDto>> MeAsync(CancellationToken ct) =>
        _authorized.GetAsync<UserDto>("api/auth/me", ct);

    public Task<Result<AuthenticationResult>> LoginWithGoogleAsync(string? idToken, string? accessToken, CancellationToken ct) =>
        _anonymous.PostAsync<AuthenticationResult, GoogleLoginPayload>("api/auth/external/Google",
            new GoogleLoginPayload(idToken, accessToken), ct);

    public sealed record GoogleLoginPayload(string? IdToken, string? AccessToken);

    private sealed class AnonymousApiClient(IHttpClientFactory httpClientFactory)
        : BaseApiClient(httpClientFactory, HttpClientNames.ApiRaw);

    private sealed class AuthorizedApiClient(IHttpClientFactory httpClientFactory)
        : BaseApiClient(httpClientFactory, HttpClientNames.Api);
}
