namespace ProductManagement.Application.Messaging;

public interface IMessageConsumer
{
    void Subscribe<T>(string queueName, Func<T, Task> handler) where T : class;
    void StartConsuming();
}