namespace RepoRemoto.Cache;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task<bool> RemoveAsync(string key);
    Task ClearAsync();
}