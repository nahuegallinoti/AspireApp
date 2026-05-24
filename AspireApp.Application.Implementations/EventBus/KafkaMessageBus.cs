using System.Text.Json;
using AspireApp.Application.Contracts.EventBus;
using AspireApp.Domain.ROP;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace AspireApp.Application.Implementations.EventBus;

/// <summary>
/// Kafka implementation of <see cref="IMessageBus"/>.
/// Producer is provided by the Aspire Confluent.Kafka integration (singleton <see cref="IProducer{TKey,TValue}"/>).
/// </summary>
internal sealed class KafkaMessageBus(
    IProducer<string, string> producer,
    ILogger<KafkaMessageBus> logger) : IMessageBus
{
    public async Task<Result<string>> PublishAsync<TMessage>(TMessage message, string topic, CancellationToken ct = default)
        where TMessage : class
    {
        var json = JsonSerializer.Serialize(message);
        var kafkaMessage = new Message<string, string>
        {
            Key = Guid.NewGuid().ToString("N"),
            Value = json
        };

        var deliveryResult = await producer.ProduceAsync(topic, kafkaMessage, ct);

        logger.LogInformation(
            "Published message to Kafka topic '{Topic}' partition {Partition} offset {Offset}",
            deliveryResult.Topic, deliveryResult.Partition.Value, deliveryResult.Offset.Value);

        return Result.Success(json);
    }
}
