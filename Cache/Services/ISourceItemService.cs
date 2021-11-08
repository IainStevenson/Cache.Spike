using Cache.Models;
using System;
using System.Threading.Tasks;
namespace Cache.Services
{
    public interface ISourceItemService
    {
        Task<CacheItem> Get(Uri resourceIdentifier);
    }
}