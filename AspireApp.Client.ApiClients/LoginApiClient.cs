using AspireApp.Api.Models.Auth;
using AspireApp.Api.Models.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Client.ApiClients;

public class LoginApiClient(IHttpClientFactory httpClientFactory) : BaseApiClient(httpClientFactory, "ApiClient")
{
    public async Task<Result<AuthenticationResult>> LoginAsync(UserLogin user, CancellationToken cancellationToken) =>
        await PostAsync<AuthenticationResult, UserLogin>("api/auth/login", user, cancellationToken);

}