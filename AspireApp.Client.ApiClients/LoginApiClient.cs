using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.ROP;

namespace AspireApp.Client.ApiClients;

public sealed class LoginApiClient(IHttpClientFactory httpClientFactory)
    : BaseApiClient(httpClientFactory, HttpClientNames.Api)
{
    public Task<Result<AuthenticationResult>> LoginAsync(UserLogin user, CancellationToken ct) =>
        PostAsync<AuthenticationResult, UserLogin>("api/auth/login", user, ct);
}
