using AutoMapper;
using Cache.Models;
using Cache.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cache.Persistence
{
    // <summary>
    // Dapper: https://dapper-tutorial.net/
    // Automapper: https://docs.automapper.org/en/stable/Getting-started.html#how-do-i-use-automapper
    // </summary>

    public class DatabaseCachedContentRepository : ICacheItemRepository
    {
        private IDapperWrapper _orm = null;
        private IMapper _mapper;


        /// <summary>
        /// Contructs the class with the required injected dependencies.
        /// </summary>
        /// <param name="orm">The database connectio via an object-to-relational mapper.</param>
        /// <param name="mapper">An instance of a configured POCO class mapper.</param>
        public DatabaseCachedContentRepository(IDapperWrapper orm, IMapper mapper)
        {
            _orm = orm;
            _mapper = mapper;
        }

        /// <summary>
        /// Get a specifc item of cached content from the database and map it to the required type, or return null
        /// </summary>
        /// <param name="resourceIdentifier">The resource identifier value of the content.</param>
        /// <returns>An instnce of <see cref="Task"/> wrapping an instance of type <see cref="CacheItem"/>.</returns>
        public async Task<CacheItem> Get(Uri resourceIdentifier)
        {
            if (string.IsNullOrWhiteSpace(resourceIdentifier?.ToString() ?? string.Empty))
            {
                throw new ArgumentNullException(nameof(resourceIdentifier));
            }
            var content = (await _orm.QueryAsync<CachedContent>(
                    CachedContentQueries.GETCACHEDCONTENT,
                    new { resourceIdentifier = resourceIdentifier.ToString() }
                    )
                ).SingleOrDefault();
            return content == null ? null : _mapper.Map<CachedContent, CacheItem>(content);
        }

        /// <summary>
        /// Retrieve all of the existing <see cref="CachedContent"/> mapped as a Dictonary usable by caches. items
        /// </summary>
        /// <returns></returns>
        public async Task<IDictionary<Uri, CacheItem>> GetAll()
        {
            // to do generate map of collection of CachedContent to Dictionary<Uri, CachedItem>
            // using ... x => new KeyValuePair( new Uri(x.Uri), Map<CachedContent, CachedItem>(x) )
            var content = (await _orm.QueryAsync<CachedContent>(CachedContentQueries.GETALLCACHEDCONTENT));
            return _mapper.Map<IEnumerable<CachedContent>, Dictionary<Uri, CacheItem>>(content);
        }

        /// <summary>
        /// Remove a Cached Item by its identifier.
        /// </summary>
        /// <param name="resourceIdentifier">The resoruce identifier of the item.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task Remove(Uri resourceIdentifier)
        {
            if (string.IsNullOrWhiteSpace(resourceIdentifier?.ToString() ?? string.Empty))
            {
                throw new ArgumentNullException(nameof(resourceIdentifier));
            }
            await _orm.Execute(CachedContentQueries.DELETECACHEDCONTENT, new { resourceIdentifier });
        }

        /// <summary>
        /// Remove all of the <see cref="CachedContent"/> items in the database.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task RemoveAll()
        {
            await _orm.Execute(CachedContentQueries.DELETEALLCACHEDCONTENT);
        }

        /// <summary>
        /// Set, create or udpate a <see cref="CachedContent"/> item by its resoruce identifier.
        /// </summary>
        /// <param name="resourceIdentifier">The resoruce identifier of the item.</param>
        /// <param name="item">The instance of <see cref="CacheItem"/>. </param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task Set(Uri resourceIdentifier, CacheItem item)
        {
            if (string.IsNullOrWhiteSpace(resourceIdentifier?.ToString() ?? string.Empty))
            {
                throw new ArgumentNullException(nameof(resourceIdentifier));
            }
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var itemToSet = _mapper.Map<CacheItem, CachedContent>(item);
            itemToSet.Uri = resourceIdentifier.ToString();

            await _orm.Execute(CachedContentQueries.SETCACHEDCONTENT, itemToSet);
        }

        /// <summary>
        /// Set, create or udpate a <see cref="CachedContent"/> items from a dictionary of items.
        /// </summary>
        /// <param name="items">A dictionary of <see cref="CacheItem"/> with which set operations are performed.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task SetAll(IDictionary<Uri, CacheItem> items)
        {
            foreach (var item in items)
            {
                await Set(item.Key, item.Value);
            }
        }
    }
}
