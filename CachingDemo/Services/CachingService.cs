namespace CachingDemo.Services
{
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Caching service that handles memory caching operations.
    /// </summary>
    public interface ICachingService
    {
        T GetOrAdd<T>(string cacheKey, Func<T> getItemCallback, TimeSpan absoluteExpiration);
        void Remove(string cacheKey);
    }

    /// <summary>
    /// Provides methods to cache and retrieve data using IMemoryCache.
    /// </summary>
    public class CachingService : ICachingService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachingService> _logger;

        public CachingService(IMemoryCache cache, ILogger<CachingService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves an item from cache if available; otherwise, fetches it and caches it.
        /// </summary>
        public T GetOrAdd<T>(string cacheKey, Func<T> getItemCallback, TimeSpan absoluteExpiration)
        {
            try
            {
                if (!_cache.TryGetValue(cacheKey, out T cacheEntry))
                {
                    _logger.LogInformation($"Cache miss for key: {cacheKey}. Fetching data...");
                    cacheEntry = getItemCallback(); // Fetches the data
                    _cache.Set(cacheKey, cacheEntry, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = absoluteExpiration
                    });
                }
                else
                {
                    _logger.LogInformation($"Cache hit for key: {cacheKey}.");
                }
                return cacheEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while retrieving cache: {ex.Message}");
                throw new CacheException("Error occurred while accessing the cache.", ex);
            }
        }

        /// <summary>
        /// Removes an item from the cache based on the cache key.
        /// </summary>
        public void Remove(string cacheKey)
        {
            try
            {
                _logger.LogInformation($"Removing cache for key: {cacheKey}.");
                _cache.Remove(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing cache: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Custom exception to handle caching-related errors.
    /// </summary>
    public class CacheException : Exception
    {
        public CacheException(string message, Exception innerException) : base(message, innerException) { }
    }

}
