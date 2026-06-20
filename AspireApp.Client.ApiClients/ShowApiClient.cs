using AspireApp.Application.Models.App;
namespace AspireApp.Client.ApiClients;

public sealed class ShowApiClient(IHttpClientFactory httpClientFactory)
    : BaseCrudApiClient<Show, long>(httpClientFactory, HttpClientNames.Api, "show");
