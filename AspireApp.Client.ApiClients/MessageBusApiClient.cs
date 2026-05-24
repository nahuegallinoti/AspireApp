using AspireApp.Application.Models.EventBus;
using AspireApp.Domain.ROP;

namespace AspireApp.Client.ApiClients;

public sealed class MessageBusApiClient(IHttpClientFactory httpClientFactory)
    : BaseApiClient(httpClientFactory, HttpClientNames.Api)
{
    public Task<Result<string>> SendAsync(EventMessage message, CancellationToken ct) =>
        PostAsync<string, EventMessage>("api/messagebus/send", message, ct);
}
