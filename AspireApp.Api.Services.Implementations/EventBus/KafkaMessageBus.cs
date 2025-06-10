using AspireApp.Application.Contracts.EventBus;
using AspireApp.Core.ROP;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class KafkaMessageBus : IMessageBus
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaMessageBus> _logger;

    public KafkaMessageBus(IConfiguration config, ILogger<KafkaMessageBus> logger)
    {
        var conf = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:29092"
        };

        _producer = new ProducerBuilder<string, string>(conf).Build();
        _logger = logger;
    }

    public async Task<Result<string>> PublishAsync<TMessage>(TMessage message, string topic)
        where TMessage : class
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = json
            });

            _logger.LogInformation($"Mensaje enviado a Kafka (topic={topic}): {json}");
            return json.Success();
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, $"Error al publicar en Kafka: {ex.Message}");
            return Result.Failure<string>("Error al enviar mensaje a Kafka");
        }
    }
}
