using System;
using System.Threading.Tasks;

namespace ProductManagement.Infrastructure.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByPrefixAsync(string prefix);
    Task<bool> ExistsAsync(string key);
}