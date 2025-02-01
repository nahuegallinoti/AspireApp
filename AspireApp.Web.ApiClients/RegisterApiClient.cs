using AspireApp.Api.Domain.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Web.ApiClients;

public class RegisterApiClient(IHttpClientFactory httpClientFactory) : BaseApiClient(httpClientFactory, "ApiClient")
{
    public async Task<Result<Guid>> RegisterAsync(UserRegister userRegister, CancellationToken cancellationToken = default) =>
        await PostAsync<Guid, UserRegister>("api/auth/register", userRegister, cancellationToken);
}