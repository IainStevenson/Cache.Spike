using System;
using System.Threading.Tasks;
namespace Cache
{
    /// <summary>
    /// Defines the cached item retrieval orchestrator.
    /// </summary>
    public interface ICacheOrchestrator
    {
        /// <summary>
        /// Handle the retrieval of a cache item based on its resource identifier.
        /// </summary>
        /// <param name="uri">The resource identifier </param>
        /// <returns>An instance of <see cref="Task{TResult}"/> delivering an <see cref="object"/>.</returns>
        Task<object> Handle(Uri uri);
    }
}