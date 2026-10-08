using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using RepoRemoto.Notifications;

namespace RepoRemoto.Cache;

public class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    public Task<T?> GetAsync<T>(string key) where T : class
    {
        var exists = cache.TryGetValue(key, out T? value);
        if (!exists)
        {
            return Task.FromResult<T?>(null);
        }

        return Task.FromResult(value);
    }
    
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }

        cache.Set(key, value, options);
        return Task.CompletedTask;
    }
    
    public Task<bool> RemoveAsync(string key)
    {
        var exists = cache.TryGetValue(key, out var x);
        if (exists)
        {
            cache.Remove(key);
        }

        return Task.FromResult(exists);
    }
    
    public Task ClearAsync()
    {
        cache.Dispose();
        cache = new MemoryCache(new MemoryCacheOptions());
        return Task.CompletedTask;
    }
}