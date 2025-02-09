using AspireApp.Api.Models.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Client.ApiClients;

public class RegisterApiClient(IHttpClientFactory httpClientFactory) : BaseApiClient(httpClientFactory, "ApiClient")
{
    public async Task<Result<Guid>> RegisterAsync(UserRegister userRegister, CancellationToken cancellationToken) =>
        await PostAsync<Guid, UserRegister>("api/auth/register", userRegister, cancellationToken);
}