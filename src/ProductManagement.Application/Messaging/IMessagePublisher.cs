namespace ProductManagement.Application.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string queueName) where T : class;
}