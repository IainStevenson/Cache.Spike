using System;
using System.Diagnostics.CodeAnalysis;

namespace Cache.Services
{
    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class CachingPolicy : ICachingPolicy
    {
        /// <inheritdoc/>
        public TimeSpan ExpireAfter { get; set; } = new TimeSpan(7,0,0,0);

        /// <inheritdoc/>
        public bool ReUseLatestExpired { get; set; } = true;        
    }
}