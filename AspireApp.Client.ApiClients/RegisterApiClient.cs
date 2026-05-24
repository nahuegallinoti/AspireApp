using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.ROP;

namespace AspireApp.Client.ApiClients;

public sealed class RegisterApiClient(IHttpClientFactory httpClientFactory)
    : BaseApiClient(httpClientFactory, HttpClientNames.Api)
{
    public Task<Result<Guid>> RegisterAsync(UserRegister userRegister, CancellationToken ct) =>
        PostAsync<Guid, UserRegister>("api/auth/register", userRegister, ct);
}
