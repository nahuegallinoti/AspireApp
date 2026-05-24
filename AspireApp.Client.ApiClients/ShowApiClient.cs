using AspireApp.Application.Models.App;
using AspireApp.Domain.ROP;

namespace AspireApp.Client.ApiClients;

public sealed class ShowApiClient(IHttpClientFactory httpClientFactory)
    : BaseApiClient(httpClientFactory, HttpClientNames.Api)
{
    public Task<Result<Show>> GetAsync(int id, CancellationToken ct) =>
        GetAsync<Show>($"api/show/{id}", ct);

    public Task<Result<Show>> CreateAsync(Show show, CancellationToken ct) =>
        PostAsync<Show, Show>("api/show", show, ct);

    public Task<Result<Show>> UpdateAsync(Show show, CancellationToken ct) =>
        PutAsync($"api/show/{show.Id}", show, ct);

    public Task<Result<Show>> DeleteAsync(int id, CancellationToken ct) =>
        DeleteAsync<Show>($"api/show/{id}", ct);

    public Task<Result<IEnumerable<Show>>> GetAllAsync(CancellationToken ct) =>
        GetAsync<IEnumerable<Show>>("api/show", ct);
}
