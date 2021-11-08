using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Cache.Persistence
{
    [ExcludeFromCodeCoverage]
    public class DapperWrapper : IDapperWrapper
    {
        private IDbConnection _connection;
        public DapperWrapper(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task Execute(string sql, params object[] values)
        {
            await _connection.ExecuteAsync(sql, values);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] values)
        {
            return await _connection.QueryAsync<T>(sql, values);
        }
    }
}
