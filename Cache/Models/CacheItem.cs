using System;
using System.Diagnostics.CodeAnalysis;

namespace Cache.Models
{
    /// <summary>
    /// An item to be used in any type of cache. 
    /// Recording the time it was created to be used in any item expiry policies that may be employed.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CacheItem
    {
        /// <summary>
        /// A storage primary key for the item.
        /// </summary>
        public long Id { get; set; } 
        /// <summary>
        /// The exact time the item was created. Usable as a point of reference for time based expiry of the item.
        /// </summary>
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The item being cached.
        /// </summary>
        public object Content { get; set; }
    }
}