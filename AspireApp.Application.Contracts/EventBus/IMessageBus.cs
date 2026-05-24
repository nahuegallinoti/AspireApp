using AspireApp.Domain.ROP;

namespace AspireApp.Application.Contracts.EventBus;

/// <summary>
/// Broker-agnostic message bus abstraction.
/// <c>topic</c> maps to a Kafka topic or a RabbitMQ routing key depending on the configured provider.
/// </summary>
public interface IMessageBus
{
    Task<Result<string>> PublishAsync<TMessage>(TMessage message, string topic, CancellationToken ct = default)
        where TMessage : class;
}
