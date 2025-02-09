using AspireApp.Api.Models.App;
using AspireApp.Core.ROP;

namespace AspireApp.Client.ApiClients;

public class ProductApiClient(IHttpClientFactory httpClientFactory) : BaseApiClient(httpClientFactory, "ApiClient")
{
    public async Task<Result<Product>> GetProductAsync(int id, CancellationToken cancellationToken = default) =>
        await GetAsync<Product>($"api/product/{id}", cancellationToken);

    public async Task<Result<Product>> CreateProductAsync(Product product, CancellationToken cancellationToken = default) =>
        await PostAsync<Product, Product>("api/product", product, cancellationToken);

    public async Task<Result<Product>> UpdateProductAsync(Product product, CancellationToken cancellationToken = default) =>
        await PutAsync($"api/product/{product.Id}", product, cancellationToken);

    public async Task<Result<Product>> DeleteProductAsync(int id, CancellationToken cancellationToken = default) =>
        await DeleteAsync<Product>($"api/product/{id}", cancellationToken);

    public async Task<Result<IEnumerable<Product>>> GetProductsAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<IEnumerable<Product>>("api/product", cancellationToken);
}
