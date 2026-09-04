using Core.Infrastructure.Database.Queries.Interfaces;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Core.Infrastructure.Database.Queries
{
    public class KeyedQueryConnection(IServiceProvider serviceProvider, string key) : IQueryConnection
    {
        public virtual async Task<IEnumerable<T>?> QueryAsync<T>(string sqlQuery, DynamicParameters? parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            IQueryContext context = serviceProvider.GetRequiredKeyedService<IQueryContext>(key);
            IEnumerable<T>? result = await context.OpenConnection(() =>
            {
                var sqlCommand = new CommandDefinition(sqlQuery, parameters: parameters, commandType: commandType, cancellationToken: cancellationToken);
                return context.Connection?.QueryAsync<T>(sqlCommand) ?? Task.FromResult(Enumerable.Empty<T>());
            });
            return result;
        }

        public virtual async Task<T?> QueryFirstOrDefaultAsync<T>(string sqlQuery, DynamicParameters? parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            IQueryContext context = serviceProvider.GetRequiredKeyedService<IQueryContext>(key);
            T? result = await context.OpenConnection(() =>
            {
                var sqlCommand = new CommandDefinition(sqlQuery, parameters, commandType: commandType, cancellationToken: cancellationToken);
                return context.Connection?.QueryFirstOrDefaultAsync<T>(sqlCommand) ?? Task.FromResult(default(T));
            });
            return result;
        }
    }

    public class QueryConnection(IQueryContext context) : IQueryConnection
    {
        public virtual async Task<IEnumerable<T>?> QueryAsync<T>(string sqlQuery, DynamicParameters? parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        {
            IEnumerable<T>? result = await context.OpenConnection(() =>
            {
                var sqlCommand = new CommandDefinition(sqlQuery, parameters: parameters, commandType: commandType, cancellationToken: cancellationToken);
                return context.Connection?.QueryAsync<T>(sqlCommand) ?? Task.FromResult(Enumerable.Empty<T>());
            });
            return result;
        }

        public virtual async Task<T?> QueryFirstOrDefaultAsync<T>(string sqlQuery, DynamicParameters? parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        {
            T? result = await context.OpenConnection(() =>
            {
                var sqlCommand = new CommandDefinition(sqlQuery, parameters, commandType: commandType, cancellationToken: cancellationToken);
                return context.Connection?.QueryFirstOrDefaultAsync<T>(sqlCommand) ?? Task.FromResult(default(T));
            });
            return result;
        }
    }
}
