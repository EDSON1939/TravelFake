using Core.Infrastructure.Database.Queries.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Database.Queries
{
    public class KeyedQuery(IServiceProvider serviceProvider, string key) : IQuery
    {
        public async Task<T?> QuerySqlAsync<T>(IQueryBase<T> sqlQuery, CancellationToken cancellationToken = default)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            var connection = serviceProvider.GetRequiredKeyedService<IQueryConnection>(key);
            return await sqlQuery.ExecuteAsync(connection, cancellationToken);

        }
    }

    public class Query(IQueryConnection connection) : IQuery
    {
        public async Task<T?> QuerySqlAsync<T>(IQueryBase<T> sqlQuery, CancellationToken cancellationToken = default)
        {
            return await sqlQuery.ExecuteAsync(connection, cancellationToken);

        }
    }
}
