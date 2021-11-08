using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cache.Persistence
{
    public interface IDapperWrapper
    {
        Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] values);
        Task Execute(string sql, params object[] values);
    }
}
