using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileSnooper.Services
{
    public interface ICacheService
    {
        Task InsertCacheItem<T>(string key, T value);

        Task<List<ICacheEntry>> GetCacheAllCacheItems();

        Task RemoveCacheItem(string key);
    }
}