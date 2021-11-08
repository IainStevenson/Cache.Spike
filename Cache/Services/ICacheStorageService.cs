using Cache.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cache.Services
{
    public interface ICacheStorageService : ICacheService
    {
        Task<IDictionary<Uri, CacheItem>> GetAll();
    }
}