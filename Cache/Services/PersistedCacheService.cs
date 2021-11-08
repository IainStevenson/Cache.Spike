using Cache.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cache.Services
{

    public class PersistedCacheService : ICacheStorageService
    {
        private readonly ICacheItemRepository _repository;
        public PersistedCacheService(ICacheItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<CacheItem> Get(Uri resourceIdentifier)
        {
            if (string.IsNullOrWhiteSpace(resourceIdentifier?.ToString() ?? string.Empty))
            {
                throw new ArgumentNullException(nameof(resourceIdentifier));
            }
            return await _repository.Get(resourceIdentifier);
        }

        public async Task<IDictionary<Uri, CacheItem>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task Remove(Uri resourceIdentifier)
        {
            if (string.IsNullOrWhiteSpace(resourceIdentifier?.ToString() ?? string.Empty))
            {
                throw new ArgumentNullException(nameof(resourceIdentifier));
            }
            await _repository.Remove(resourceIdentifier);
        }

        public async Task RemoveAll()
        {
            await _repository.RemoveAll();
        }

        public async Task Set(Uri resourceIdentifier, CacheItem item)
        {
            if (string.IsNullOrWhiteSpace(resourceIdentifier?.ToString() ?? string.Empty))
            {
                throw new ArgumentNullException(nameof(resourceIdentifier));
            }
            await _repository.Set(resourceIdentifier, item);

        }

        public async Task SetAll(IDictionary<Uri, CacheItem> items)
        {
            await _repository.SetAll(items);
        }
    }
}