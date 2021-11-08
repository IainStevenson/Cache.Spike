using Cache.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Cache.Services
{
    /// <summary>
    /// Implements <see cref="ICacheService"/> for in memory storage using a threadsafe <see cref="ConcurrentDictionary{TKey, TResult}"/>.
    /// </summary>
    public class InMemoryCacheService : ICacheService
    {
        private readonly ConcurrentDictionary<Uri, CacheItem> _cache = new ConcurrentDictionary<Uri, CacheItem>();
        
        /// <inheritdoc/>
        public Task<CacheItem> Get(Uri resourceIdentifier)
        {
            CacheItem item;
            _cache.TryGetValue(resourceIdentifier, out item);
            return Task.FromResult(item);
        }
        /// <inheritdoc/>
        public Task Set(Uri resourceIdentifier, CacheItem item)
        {
            _cache.AddOrUpdate(resourceIdentifier, item, (k, v) => item);
            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        public Task RemoveAll()
        {
            _cache.Clear();
            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        public Task Remove(Uri resourceIdentifier)
        {
            _ = _cache.TryRemove(resourceIdentifier, out _);
            return Task.FromResult(0);
        }

        public Task SetAll(IDictionary<Uri, CacheItem> items)
        {
            foreach (var item in items)
            {
                _cache.AddOrUpdate(item.Key, item.Value, (k, v) => item.Value);
            }
            return Task.FromResult(0);
        }
    }
}