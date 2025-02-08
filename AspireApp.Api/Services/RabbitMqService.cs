using AspireApp.Api.Domain.Rabbit;
using AspireApp.Application.Implementations.Extensions;
using AspireApp.Core.ROP;
using RabbitMQ.Client;
using System.Text;

namespace AspireApp.Api.Services;

public class RabbitMqService
{
    private readonly ConnectionFactory _factory;

    public RabbitMqService()
    {
        _factory = new ConnectionFactory
        {
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            HostName = "localhost",
            Port = 5672
        };
    }

    public Task<Result<string>> SendMessage(RabbitMessage message)
    {
        return ValidateMessage(message)
               .Bind(PublishMessage);
    }
    private static Result<RabbitMessage> ValidateMessage(RabbitMessage message) => message.Validate();

    public async Task<Result<string>> PublishMessage(RabbitMessage message)
    {
        try
        {
            await using var connection = await _factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "test_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(message.Message);

            await channel.BasicPublishAsync(exchange: "", routingKey: "test_queue", body);

            return message.Message;
        }

        catch (Exception ex)
        {
            return Result.Failure<string>(ex.Message);
        }
    }

}
