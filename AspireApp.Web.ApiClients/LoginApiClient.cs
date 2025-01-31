using AspireApp.Api.Domain.Auth;
using AspireApp.Api.Domain.Auth.User;
using System.Net.Http.Json;

namespace AspireApp.Web.ApiClients;

public class LoginApiClient(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("ApiClient");

    public async Task<AuthenticationResult> LoginAsync(UserLogin user, CancellationToken cancellationToken = default)
    {
        UserLogin loginRequest = new()
        {
            Email = user.Email,
            Password = user.Password
        };

        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginRequest, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthenticationResult>(cancellationToken);

        return result;
    }
}