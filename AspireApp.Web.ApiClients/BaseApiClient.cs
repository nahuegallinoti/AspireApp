using AspireApp.Core.ROP;
using System.Collections.Immutable;
using System.Net.Http.Json;

namespace AspireApp.Web.ApiClients;

/// <summary>
/// Base API client for handling HTTP requests.
/// </summary>
public abstract class BaseApiClient(IHttpClientFactory httpClientFactory, string clientName)
{
    protected readonly HttpClient _httpClient = httpClientFactory.CreateClient(clientName);

    /// <summary>
    /// Sends a POST request to the specified URL with the given data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="url"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<Result<T>> PostAsync<T, U>(string url, U data, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Post, url, JsonContent.Create(data), cancellationToken);

    /// <summary>
    /// Sends a GET request to the specified URL.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<Result<T>> GetAsync<T>(string url, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Get, url, cancellationToken: cancellationToken);

    /// <summary>
    /// Sends a PUT request to the specified URL with the given data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public Task<Result<T>> PutAsync<T>(string url, T data, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Put, url, JsonContent.Create(data), cancellationToken);

    /// <summary>
    /// Sends a DELETE request to the specified URL.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public Task<Result<T>> DeleteAsync<T>(string url, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Delete, url, cancellationToken: cancellationToken);

    /// <summary>
    /// Sends a request to the specified URL with the given method and content.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="method"></param>
    /// <param name="url"></param>
    /// <param name="content"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

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

    /// <summary>
    /// Extracts the error message from the HTTP response.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>

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
