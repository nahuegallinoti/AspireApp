using AspireApp.Api.Models.Rabbit;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Contracts.Rabbit;

public interface IRabbitMqService
{
    Task<Result<string>> SendMessage(RabbitMessage message, string routingKey);
}
