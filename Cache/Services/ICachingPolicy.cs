using System;
namespace Cache.Services
{
    /// <summary>
    /// Defines the cache item handling policy.
    /// </summary>
    public interface ICachingPolicy
    {
        /// <summary>
        /// Consider items expired after they age greater than the specified time span period.
        /// </summary>
        TimeSpan ExpireAfter { get; set; }

        /// <summary>
        /// When no source item is available and all available items are expired, when true, use the latest expired as source.
        /// </summary>
        bool ReUseLatestExpired { get; set; }
    }
}