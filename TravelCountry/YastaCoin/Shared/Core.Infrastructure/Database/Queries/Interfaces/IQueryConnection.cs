using Dapper;
using System.Data;

namespace Core.Infrastructure.Database.Queries.Interfaces
{
    public interface IQueryConnection
    {
        Task<IEnumerable<T>?> QueryAsync<T>(string sqlQuery, DynamicParameters? parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);

        Task<T?> QueryFirstOrDefaultAsync<T>(string sqlQuery, DynamicParameters? parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    }
}
