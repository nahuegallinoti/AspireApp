using AspireApp.Api.Domain.Rabbit;
using AspireApp.Core.ROP;

namespace AspireApp.Web.ApiClients;

public class RabbitMqApiClient(IHttpClientFactory httpClientFactory) : BaseApiClient(httpClientFactory, "ApiClient")
{
    public async Task<Result<string>> SendMessageAsync(RabbitMessage message, CancellationToken ct)
    {
        var result = await PostAsync<string, RabbitMessage>("api/rabbitmq/send", message, ct);
        return result;
    }
}