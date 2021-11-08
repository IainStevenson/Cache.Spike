using Cache.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cache.Services
{
    public interface ICacheItemRepository
    {
        /// <summary>
        /// Return all known <see cref="CacheItem"/>.
        /// </summary>
        /// <returns>An instance ot <see cref="Task"/> wrapping a collection of zero or more <see cref="CacheItem"/>.</returns>
        Task<IDictionary<Uri, CacheItem>> GetAll();

        /// <summary>
        /// Return an instance of <see cref="CacheItem"/> indexed via specified resource identifier, or null if none exists.
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier.</param>
        /// <returns>An instance ot <see cref="Task"/> wrapping a <see cref="CacheItem"/>.</returns>
        Task<CacheItem> Get(Uri resourceIdentifier);

        /// <summary>
        /// Store (Create or Update) the <see cref="CacheItem"/> indexed by the resource identifier.
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier.</param>
        /// <param name="item">The item to store.</param>
        /// <returns>An instance ot <see cref="Task"/>.</returns>
        Task Set(Uri resourceIdentifier, CacheItem item);

        /// <summary>
        /// Add each of the provided collection of <see cref="CacheItem"/> to the store, as create or updates as needed.
        /// </summary>
        /// <param name="items">The colelction of indexed items to store.</param>
        /// <returns>An instance ot <see cref="Task"/>.</returns>
        Task SetAll(IDictionary<Uri, CacheItem> items);

        /// <summary>
        /// Remove a specific <see cref="CacheItem"/> from the store.
        /// </summary>
        /// <param name="resourceIdentifier"></param>
        /// <returns>An instance ot <see cref="Task"/>.</returns>
        Task Remove(Uri resourceIdentifier);
       
        /// <summary>
        /// REmove all <see cref="CacheItem"/> from teh store.
        /// </summary>
        /// <returns>An instance ot <see cref="Task"/>.</returns>
        Task RemoveAll();
    }
}