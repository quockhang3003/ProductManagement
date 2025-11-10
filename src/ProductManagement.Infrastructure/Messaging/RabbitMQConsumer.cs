
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using ProductManagement.Application.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductManagement.Infrastructure.Messaging;

public class RabbitMQConsumer : IMessageConsumer, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQConsumer> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();

    public RabbitMQConsumer(
        IOptions<RabbitMQSettings> settings,
        ILogger<RabbitMQConsumer> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: _settings.RetryCount,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Message processing retry {RetryCount} after {TimeSpan}s due to {Exception}",
                        retryCount, timeSpan.TotalSeconds, exception.Message);
                });

        InitializeConnection();
    }

    private void InitializeConnection()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedHeartbeat = TimeSpan.FromSeconds(60)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _logger.LogInformation("RabbitMQ Consumer connected to {HostName}:{Port}", 
                _settings.HostName, _settings.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ Consumer connection");
            throw;
        }
    }

    public void Subscribe<T>(string queueName, Func<T, Task> handler) where T : class
    {
        lock (_lock)
        {
            if (_channel == null)
            {
                _logger.LogError("Channel is null, cannot subscribe");
                return;
            }

            // Declare queue
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Set QoS
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    _logger.LogDebug("Received message from {QueueName}: {Message}", queueName, json);

                    await _retryPolicy.ExecuteAsync(async () =>
                    {
                        var message = JsonSerializer.Deserialize<T>(json);
                        if (message != null)
                        {
                            await handler(message);
                        }
                    });

                    // Acknowledge the message
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    
                    _logger.LogInformation("Message processed successfully from {QueueName}", queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from {QueueName}", queueName);
                    
                    // Reject and requeue the message
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            
            _logger.LogInformation("Started consuming from queue {QueueName}", queueName);
        }
    }

    public void StartConsuming()
    {
        _logger.LogInformation("RabbitMQ Consumer is running");
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            
            _logger.LogInformation("RabbitMQ Consumer connection disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ Consumer connection");
        }
    }
}