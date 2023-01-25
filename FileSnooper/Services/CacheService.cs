using FileSnooper.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileSnooper.Services
{
    public class CacheService : ICacheService
    {
        private readonly ILogger<CacheService> _logger;
        private readonly IMemoryCache _memCache;
        private readonly int _snoopDelayInMinutes;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 10);
        private readonly MemoryCacheEntryOptions CacheOptions;

        public CacheService(ILogger<CacheService> logger, IMemoryCache memCache)
        {
            _logger = logger;
            _memCache = memCache;

            CacheOptions = new MemoryCacheEntryOptions()
               .SetSlidingExpiration(TimeSpan.FromMinutes(15))
               .SetAbsoluteExpiration(TimeSpan.FromMinutes(25))
               .SetPriority(CacheItemPriority.Normal);
        }

        public async Task InsertCacheItem<T>(string key, T value)
        {
            try
            {
                await _semaphore.WaitAsync();

                if (_memCache.TryGetValue(key, out string cacheData))
                {
                    _logger.LogDebug("In {method}, File already in cache!. File name: {fileName}", nameof(InsertCacheItem), key);
                }
                else
                {
                    _logger.LogDebug("In {method}, File not in cache, inserting... File name: {fileName}", nameof(InsertCacheItem), key);
                    _memCache.Set(key, value, CacheOptions);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while inserting a cache item in method: {methodName}. Details: {error}", nameof(InsertCacheItem), ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task RemoveCacheItem(string key)
        {
            try
            {
                await _semaphore.WaitAsync();

                if (_memCache.TryGetValue(key, out string cacheData))
                {
                    _logger.LogDebug("In ## {method} ##, found file. Removing file name: {fileName}", nameof(RemoveCacheItem), key);
                    _memCache.Remove(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while removing a cache item in method: {methodName}. Details: {error}", nameof(RemoveCacheItem), ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<ICacheEntry>> GetCacheAllCacheItems()
        {
            try
            {
                await _semaphore.WaitAsync();

                var cacheItems = new List<string>();

                return _memCache.GetAllCacheEntries();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while retrieving all cache item in method: {methodName}. Details: {error}", nameof(GetCacheAllCacheItems), ex.Message);
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}