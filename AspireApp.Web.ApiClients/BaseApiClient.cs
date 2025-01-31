using System.Net.Http.Json;

namespace AspireApp.Web.ApiClients;

public abstract class BaseApiClient(IHttpClientFactory httpClientFactory, string clientName)
{
    protected readonly HttpClient _httpClient = httpClientFactory.CreateClient(clientName);

    protected Task<T?> PostAsync<T, U>(string url, U data, CancellationToken cancellationToken = default) =>
            SendRequestAsync<T>(HttpMethod.Post, url, JsonContent.Create(data), cancellationToken);

    protected Task<T?> PostAsync<T>(string url, T data, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Post, url, JsonContent.Create(data), cancellationToken);

    protected Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Get, url, cancellationToken: cancellationToken);

    protected Task<T?> PutAsync<T>(string url, T data, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Put, url, JsonContent.Create(data), cancellationToken);

    protected Task<T?> DeleteAsync<T>(string url, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Delete, url, cancellationToken: cancellationToken);

    private async Task<T?> SendRequestAsync<T>(HttpMethod method, string url, HttpContent? content = null, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<T>(cancellationToken)
            : default;
    }
}
