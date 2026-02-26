using AspireApp.Domain.ROP;

namespace AspireApp.Application.Contracts.EventBus;

public interface IMessageBus
{
    /// <summary>
    /// Publica un mensaje al event bus
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="topic"></param>
    /// <returns></returns>

    //Topic para Kafka - Routing key para RabbitMQ
    Task<Result<string>> PublishAsync<TMessage>(TMessage message, string topic)
        where TMessage: class;

}