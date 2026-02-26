using AspireApp.Application.Models.Rabbit;
using AspireApp.Domain.ROP;

namespace AspireApp.Client.ApiClients;

public class RabbitMqApiClient(IHttpClientFactory httpClientFactory) : BaseApiClient(httpClientFactory, "ApiClient")
{
    public async Task<Result<string>> SendMessageAsync(RabbitMessage message, CancellationToken ct)
    {
        var result = await PostAsync<string, RabbitMessage>("api/messagebus/send", message, ct);
        return result;
    }
}