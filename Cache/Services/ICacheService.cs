using Cache.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Cache.Services
{
    /// <summary>
    /// Defines the behaviours of all Cache implementations.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Retrieve the cache item for the specified resource identifier.
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier.</param>
        /// <returns>An intance of <see cref="Task{TResult}"/></returns>
        Task<CacheItem> Get(Uri resourceIdentifier);

        /// <summary>
        /// Accept and store a cache item for the resource identifier.
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier.</param>
        /// <param name="item">The cache item.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task Set(Uri resourceIdentifier, CacheItem item);

        /// <summary>
        /// Empty the cache list of all entries.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task RemoveAll();

        /// <summary>
        /// Remove a specific item using its resource identifier.
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task Remove(Uri resourceIdentifier);

        /// <summary>
        /// Adds all of the cache items passed to the cache using Add Or Update.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task SetAll(IDictionary<Uri, CacheItem> items);
    }
}