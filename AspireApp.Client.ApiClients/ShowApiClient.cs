using AspireApp.Api.Models.App;
using AspireApp.Core.ROP;

namespace AspireApp.Client.ApiClients;

public class ShowApiClient(IHttpClientFactory httpClientFactory) : BaseApiClient(httpClientFactory, "ApiClient")
{
    public async Task<Result<Show>> GetShowAsync(int id, CancellationToken cancellationToken = default) =>
        await GetAsync<Show>($"api/show/{id}", cancellationToken);

    public async Task<Result<Show>> CreateShowAsync(Show show, CancellationToken cancellationToken = default) =>
        await PostAsync<Show, Show>("api/show", show, cancellationToken);

    public async Task<Result<Show>> UpdateShowAsync(Show show, CancellationToken cancellationToken = default) =>
        await PutAsync($"api/show/{show.Id}", show, cancellationToken);

    public async Task<Result<Show>> DeleteShowAsync(int id, CancellationToken cancellationToken = default) =>
        await DeleteAsync<Show>($"api/show/{id}", cancellationToken);

    public async Task<Result<IEnumerable<Show>>> GetShowsAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<IEnumerable<Show>>("api/show", cancellationToken);
}
