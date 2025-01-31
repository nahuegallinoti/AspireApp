using AspireApp.Api.Domain.Auth.User;

namespace AspireApp.Web.ApiClients;

public class RegisterApiClient(IHttpClientFactory httpClientFactory) : BaseApiClient(httpClientFactory, "ApiClient")
{
    public async Task<UserRegister?> RegisterAsync(UserRegister userRegister, CancellationToken cancellationToken = default) =>
        await PostAsync("api/auth/register", userRegister, cancellationToken);
}