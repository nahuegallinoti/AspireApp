using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Contracts.EventBus;

public enum MessageBusProvider
{
    RabbitMq,
    Kafka
}

public sealed class MessageBusOptions
{
    public const string SectionName = "EventBus";

    /// <summary>Broker selection: RabbitMq or Kafka.</summary>
    [Required]
    public MessageBusProvider Provider { get; set; } = MessageBusProvider.RabbitMq;

    /// <summary>Default exchange (RabbitMQ) used when publishing.</summary>
    public string DefaultExchange { get; set; } = "aspireapp.events";

    /// <summary>Connection-name used by Aspire integration (Rabbit/Kafka).</summary>
    public string ConnectionName { get; set; } = "messaging";
}
