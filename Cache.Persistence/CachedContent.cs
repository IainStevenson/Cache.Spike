using System.Diagnostics.CodeAnalysis;

namespace Cache.Persistence
{
    [ExcludeFromCodeCoverage]
    public class CachedContent
    {
        public long Id { get; set; }

        public string Uri { get; set; }

        public System.DateTimeOffset Created { get; set; }

        public byte[] Content { get; set; }

        public string MediaType { get; set; }
    }
}