using AspireApp.Core.ROP;
using System.Collections.Immutable;
using System.Net.Http.Json;

namespace AspireApp.Web.ApiClients;

public abstract class BaseApiClient(IHttpClientFactory httpClientFactory, string clientName)
{
    protected readonly HttpClient _httpClient = httpClientFactory.CreateClient(clientName);

    public Task<Result<T>> PostAsync<T, U>(string url, U data, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Post, url, JsonContent.Create(data), cancellationToken);

    public Task<Result<T>> GetAsync<T>(string url, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Get, url, cancellationToken: cancellationToken);

    public Task<Result<T>> PutAsync<T>(string url, T data, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Put, url, JsonContent.Create(data), cancellationToken);

    public Task<Result<T>> DeleteAsync<T>(string url, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Delete, url, cancellationToken: cancellationToken);

    private async Task<Result<T>> SendRequestAsync<T>(HttpMethod method, string url, HttpContent? content = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(method, url) { Content = content };
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await ExtractErrorMessage(response);
                return Result.Failure<T>(errorMessage, response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);

            return result is not null
                ? Result.Success(result, response.StatusCode)
                : Result.Failure<T>(["Respuesta vacía o nula del servidor."], response.StatusCode);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>([$"Error inesperado: {ex.Message}"]);
        }
    }

    private static async Task<ImmutableArray<string>> ExtractErrorMessage(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return [$"Error HTTP {response.StatusCode}"];

            return [content];
        }

        catch
        {
            return ["Ocurrió un error desconocido al procesar la respuesta del servidor."];
        }
    }
}
