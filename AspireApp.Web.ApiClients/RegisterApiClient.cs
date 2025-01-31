using AspireApp.Api.Domain.Auth.User;
using System.Net.Http.Json;

namespace AspireApp.Web.ApiClients;

public class RegisterApiClient(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("ApiClient");

    public async Task<UserRegister?> RegisterAsync(UserRegister userRegister, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", userRegister, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserRegister>(cancellationToken);

        return result;
    }
}