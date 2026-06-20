using AspireApp.Application.Models;
using AspireApp.Domain.ROP;

namespace AspireApp.Client.ApiClients;

public abstract class BaseCrudApiClient<TModel, TID>(
    IHttpClientFactory httpClientFactory,
    string clientName,
    string resource) : BaseApiClient(httpClientFactory, clientName)
    where TModel : BaseModel<TID>
    where TID : struct
{
    public Task<Result<IEnumerable<TModel>>> GetAllAsync(CancellationToken ct) =>
        GetAsync<IEnumerable<TModel>>($"api/{resource}", ct);

    public Task<Result<TModel>> GetAsync(TID id, CancellationToken ct) =>
        GetAsync<TModel>($"api/{resource}/{id}", ct);

    public Task<Result<TModel>> CreateAsync(TModel model, CancellationToken ct) =>
        PostAsync<TModel, TModel>($"api/{resource}", model, ct);

    public Task<Result<TModel>> UpdateAsync(TModel model, CancellationToken ct) =>
        PutAsync($"api/{resource}/{model.Id}", model, ct);

    public Task<Result<TModel>> DeleteAsync(TID id, CancellationToken ct) =>
        DeleteAsync<TModel>($"api/{resource}/{id}", ct);
}
