using AspireApp.Application.Models.App;
namespace AspireApp.Client.ApiClients;

public sealed class ProductApiClient(IHttpClientFactory httpClientFactory)
    : BaseCrudApiClient<Product, long>(httpClientFactory, HttpClientNames.Api, "product");
