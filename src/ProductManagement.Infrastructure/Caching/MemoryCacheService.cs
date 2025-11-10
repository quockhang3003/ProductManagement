using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ProductManagement.Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;
    private static readonly HashSet<string> _cacheKeys = new();
    private static readonly object _lock = new();

    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("Memory cache HIT for key: {Key}", key);
                return Task.FromResult(value);
            }

            _logger.LogDebug("Memory cache MISS for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting memory cache for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30),
                Priority = CacheItemPriority.Normal
            };

            _memoryCache.Set(key, value, cacheOptions);
            
            lock (_lock)
            {
                _cacheKeys.Add(key);
            }

            _logger.LogDebug("Memory cache SET for key: {Key} with expiration: {Expiration}", 
                key, expiration ?? TimeSpan.FromMinutes(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting memory cache for key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            
            lock (_lock)
            {
                _cacheKeys.Remove(key);
            }

            _logger.LogDebug("Memory cache REMOVED for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing memory cache for key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            List<string> keysToRemove;
            
            lock (_lock)
            {
                keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix)).ToList();
            }

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                
                lock (_lock)
                {
                    _cacheKeys.Remove(key);
                }
            }

            _logger.LogDebug("Memory cache REMOVED {Count} keys with prefix: {Prefix}", 
                keysToRemove.Count, prefix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing memory cache by prefix: {Prefix}", prefix);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }
}