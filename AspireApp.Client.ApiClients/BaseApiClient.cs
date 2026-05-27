using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AspireApp.Domain.ROP;

namespace AspireApp.Client.ApiClients;

public record ProblemDetails(int Status, string? Detail, string? Type, string? Title, string? Instance);

public abstract class BaseApiClient(IHttpClientFactory httpClientFactory, string clientName)
{
    private readonly HttpClient HttpClient = httpClientFactory.CreateClient(clientName);

    private static readonly JsonSerializerOptions ProblemJson = new() { PropertyNameCaseInsensitive = true };

    public Task<Result<T>> PostAsync<T, TR>(string url, TR data, CancellationToken ct) =>
        SendAsync<T>(HttpMethod.Post, url, JsonContent.Create(data), ct);

    public Task<Result<T>> GetAsync<T>(string url, CancellationToken ct) =>
        SendAsync<T>(HttpMethod.Get, url, content: null, ct);

    public Task<Result<T>> PutAsync<T>(string url, T data, CancellationToken ct) =>
        SendAsync<T>(HttpMethod.Put, url, JsonContent.Create(data), ct);

    public Task<Result<TOut>> PutAsync<TOut, TIn>(string url, TIn data, CancellationToken ct) =>
        SendAsync<TOut>(HttpMethod.Put, url, JsonContent.Create(data), ct);

    public Task<Result<T>> DeleteAsync<T>(string url, CancellationToken ct) =>
        SendAsync<T>(HttpMethod.Delete, url, content: null, ct);

    private async Task<Result<T>> SendAsync<T>(HttpMethod method, string url, HttpContent? content, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, url) { Content = content };
        using var response = await HttpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errors = await ReadErrorAsync(response, ct);
            return Result.Failure<T>(errors, response.StatusCode);
        }

        if (response.StatusCode == HttpStatusCode.NoContent
            || (response.Content.Headers.ContentLength is 0 or null && (method == HttpMethod.Delete || method == HttpMethod.Put)))
        {
            T empty = default!;
            return empty.Success(response.StatusCode);
        }

        try
        {
            var value = await response.Content.ReadFromJsonAsync<T>(ct);
            return value is not null
                ? value.Success(response.StatusCode)
                : Result.Failure<T>("Empty response from server.", response.StatusCode);
        }
        catch (JsonException)
        {
            var raw = await response.Content.ReadAsStringAsync(ct);
            return string.IsNullOrWhiteSpace(raw)
                ? Result.Failure<T>("Empty response from server.", response.StatusCode)
                : ((T)(object)raw).Success(response.StatusCode);
        }
    }

    private static async Task<ImmutableArray<string>> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode is HttpStatusCode.Unauthorized)
            return ["Invalid credentials."];

        var content = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(content))
            return [$"HTTP {(int)response.StatusCode} {response.ReasonPhrase}"];

        try
        {
            var problem = JsonSerializer.Deserialize<ProblemDetails>(content, ProblemJson);
            if (!string.IsNullOrEmpty(problem?.Detail))
                return [problem.Detail];
        }
        catch (JsonException) { /* fall through */ }

        return [content];
    }
}
