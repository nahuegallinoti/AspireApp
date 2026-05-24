using AspireApp.Application.Models.App;
using AspireApp.Domain.ROP;

namespace AspireApp.Client.ApiClients;

public sealed class ProductApiClient(IHttpClientFactory httpClientFactory)
    : BaseApiClient(httpClientFactory, HttpClientNames.Api)
{
    public Task<Result<Product>> GetAsync(int id, CancellationToken ct) =>
        GetAsync<Product>($"api/product/{id}", ct);

    public Task<Result<Product>> CreateAsync(Product product, CancellationToken ct) =>
        PostAsync<Product, Product>("api/product", product, ct);

    public Task<Result<Product>> UpdateAsync(Product product, CancellationToken ct) =>
        PutAsync($"api/product/{product.Id}", product, ct);

    public Task<Result<Product>> DeleteAsync(int id, CancellationToken ct) =>
        DeleteAsync<Product>($"api/product/{id}", ct);

    public Task<Result<IEnumerable<Product>>> GetAllAsync(CancellationToken ct) =>
        GetAsync<IEnumerable<Product>>("api/product", ct);
}
