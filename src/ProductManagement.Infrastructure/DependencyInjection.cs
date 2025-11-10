using Microsoft.Extensions.DependencyInjection;
using ProductManagement.Application.Messaging;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Repositories;
using ProductManagement.Infrastructure.Caching;
using ProductManagement.Infrastructure.Data;
using ProductManagement.Infrastructure.Messaging;
using ProductManagement.Infrastructure.Repositories;
using StackExchange.Redis;

namespace ProductManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string? redisConnection = null)
    {
        // Database
        services.AddSingleton<DapperContext>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        // RabbitMQ
        services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
        services.AddSingleton<IMessageConsumer, RabbitMQConsumer>();

        // Caching
        services.AddMemoryCache();
        services.AddSingleton<MemoryCacheService>();

        // Redis (nếu có)
        if (!string.IsNullOrEmpty(redisConnection))
        {
            try
            {
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                    ConnectionMultiplexer.Connect(redisConnection));
                services.AddSingleton<RedisCacheService>();
                services.AddScoped<ICacheService, RedisCacheService>();
            }
            catch
            {
                // Fallback to Memory Cache nếu Redis không available
                services.AddScoped<ICacheService, MemoryCacheService>();
            }
        }
        else
        {
            services.AddScoped<ICacheService, MemoryCacheService>();
        }
        
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        return services;
    }
}