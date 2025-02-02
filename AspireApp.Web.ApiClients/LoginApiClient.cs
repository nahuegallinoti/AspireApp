using AspireApp.Api.Domain.Auth;
using AspireApp.Api.Domain.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Web.ApiClients;

public class LoginApiClient(IHttpClientFactory httpClientFactory) : BaseApiClient(httpClientFactory, "ApiClient")
{
    public async Task<Result<AuthenticationResult>> LoginAsync(UserLogin user, CancellationToken cancellationToken = default)
    {
        UserLogin loginRequest = new()
        {
            Email = user.Email,
            Password = user.Password
        };

        return await PostAsync<AuthenticationResult, UserLogin>("api/auth/login", loginRequest, cancellationToken);
    }
}