using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using StackExchange.Redis;

namespace ProductManagement.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;

        // Polly retry policy for Redis operations
        _retryPolicy = Policy
            .Handle<RedisConnectionException>()
            .Or<RedisTimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Redis retry {RetryCount} after {TimeSpan}s due to {Exception}",
                        retryCount, timeSpan.TotalSeconds, exception.Message);
                });
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                
                if (!value.HasValue)
                {
                    _logger.LogDebug("Redis cache MISS for key: {Key}", key);
                    return null;
                }

                _logger.LogDebug("Redis cache HIT for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Redis cache for key: {Key}", key);
                return null;
            }
        });
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                var expirationTime = expiration ?? TimeSpan.FromMinutes(30);
                
                await _database.StringSetAsync(key, json, expirationTime);
                
                _logger.LogDebug(
                    "Redis cache SET for key: {Key} with expiration: {Expiration}", 
                    key, expirationTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Redis cache for key: {Key}", key);
            }
        });
    }

    public async Task RemoveAsync(string key)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                await _database.KeyDeleteAsync(key);
                _logger.LogDebug("Redis cache REMOVED for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing Redis cache for key: {Key}", key);
            }
        });
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                var endpoints = _redis.GetEndPoints();
                var server = _redis.GetServer(endpoints.First());
                
                var keys = server.Keys(pattern: $"{prefix}*").ToArray();
                
                if (keys.Length > 0)
                {
                    await _database.KeyDeleteAsync(keys);
                    _logger.LogDebug(
                        "Redis cache REMOVED {Count} keys with prefix: {Prefix}", 
                        keys.Length, prefix);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing Redis cache by prefix: {Prefix}", prefix);
            }
        });
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Redis cache existence for key: {Key}", key);
                return false;
            }
        });
    }
}