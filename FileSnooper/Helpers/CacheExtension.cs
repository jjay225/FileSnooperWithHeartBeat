using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileSnooper.Helpers
{
    internal static class CacheExtension
    {
        internal static List<ICacheEntry> GetAllCacheEntries(this IMemoryCache cache)
        {
            
            var cacheEntriesCollectionDefinition = typeof(MemoryCache).GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var cacheEntriesCollection = cacheEntriesCollectionDefinition.GetValue(cache) as dynamic;
            
            List<ICacheEntry> cacheCollectionValues = new List<ICacheEntry>();

            foreach (var cacheItem in cacheEntriesCollection)
            {                
                ICacheEntry cacheItemValue = cacheItem.GetType().GetProperty("Value").GetValue(cacheItem, null);             
                cacheCollectionValues.Add(cacheItemValue);
            }

            return cacheCollectionValues;
        }
    }
}