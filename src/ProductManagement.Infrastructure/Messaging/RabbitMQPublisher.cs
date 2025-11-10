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
using RabbitMQ.Client.Exceptions;

namespace ProductManagement.Infrastructure.Messaging;

public class RabbitMQPublisher : IMessagePublisher, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();

    public RabbitMQPublisher(
        IOptions<RabbitMQSettings> settings,
        ILogger<RabbitMQPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Configure Polly retry policy
        _retryPolicy = Policy
            .Handle<BrokerUnreachableException>()
            .Or<AlreadyClosedException>()
            .WaitAndRetryAsync(
                retryCount: _settings.RetryCount,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "RabbitMQ retry {RetryCount} after {TimeSpan}s due to {Exception}",
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

            _logger.LogInformation("RabbitMQ Publisher connected to {HostName}:{Port}", 
                _settings.HostName, _settings.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ Publisher connection");
            throw;
        }
    }

    public async Task PublishAsync<T>(T message, string queueName) where T : class
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            lock (_lock)
            {
                if (_channel == null || _channel.IsClosed)
                {
                    _logger.LogWarning("RabbitMQ channel is closed, reinitializing...");
                    InitializeConnection();
                }

                // Declare queue (idempotent operation)
                _channel!.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.Type = typeof(T).Name;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                _channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: queueName,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation(
                    "Published message {MessageType} to queue {QueueName}",
                    typeof(T).Name, queueName);
            }

            await Task.CompletedTask;
        });
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            
            _logger.LogInformation("RabbitMQ Publisher connection disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ Publisher connection");
        }
    }
}