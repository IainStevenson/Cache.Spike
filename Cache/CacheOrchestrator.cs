using Cache.Models;
using Cache.Services;
using System;
using System.Threading.Tasks;
namespace Cache
{
    /// <inherit />
    public class CacheOrchestrator : ICacheOrchestrator
    {
        private readonly ICacheService _primaryCacheService;
        private readonly ICacheStorageService _secondaryCacheService;
        private readonly ISourceItemService _sourceItemService;
        private readonly ICachingPolicy _cachingPolicy;


        /// <summary>
        /// Constructs the class.
        /// </summary>
        /// <param name="primaryCacheService">The primary cache service.</param>
        /// <param name="secondaryCacheService">The secondary cache service.</param>
        /// <param name="sourceItemService">The source item service.</param>
        /// <param name="cachingPolicy">The cache item management policy.</param>
        public CacheOrchestrator(ICacheService primaryCacheService, ICacheStorageService secondaryCacheService, ISourceItemService sourceItemService, ICachingPolicy cachingPolicy)
        {
            _primaryCacheService = primaryCacheService;
            _secondaryCacheService = secondaryCacheService;
            _sourceItemService = sourceItemService;
            _cachingPolicy = cachingPolicy;
        }

        /// <inherit />
        public async Task<object> Handle(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            var primaryResponse = await _primaryCacheService.Get(uri);

            if (IsExpired(primaryResponse))
            {
                var secondaryResponse = await _secondaryCacheService.Get(uri);

                if (IsExpired(secondaryResponse))
                {
                    var sourceResponse = await _sourceItemService.Get(uri);
                    if (sourceResponse != null)
                    {
                        await _secondaryCacheService.Set(uri, sourceResponse);
                        await _primaryCacheService.Set(uri, sourceResponse);
                        return sourceResponse.Content;
                    }
                    else
                    {
                        return await LatestOfOrNull(uri, primaryResponse, secondaryResponse);
                    }
                }
                else
                {
                    await _primaryCacheService.Set(uri, secondaryResponse);
                    return secondaryResponse.Content;
                }
            }
            return primaryResponse.Content;
        }

        private async Task<object> LatestOfOrNull(Uri uri, CacheItem primaryResponse, CacheItem secondaryResponse)
        {
            if (_cachingPolicy.ReUseLatestExpired)
            {
                var response = (primaryResponse?.Created ?? DateTimeOffset.MinValue) >=
                                    (secondaryResponse?.Created ?? DateTimeOffset.MinValue) ? primaryResponse : secondaryResponse;

                if (response != null)
                {
                    response.Created = DateTimeOffset.UtcNow;
                    await _secondaryCacheService.Set(uri, response);
                    await _primaryCacheService.Set(uri, response);
                    return response.Content;
                }
            }


            return null;
        }

        private bool IsExpired(CacheItem item)
        {
            return (item?.Created ?? DateTimeOffset.MinValue).Add(_cachingPolicy.ExpireAfter) < DateTimeOffset.UtcNow;
        }
    }
}