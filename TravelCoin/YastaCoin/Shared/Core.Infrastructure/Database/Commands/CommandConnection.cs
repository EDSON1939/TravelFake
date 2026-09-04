using Core.Infrastructure.Database.Commands.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Core.Infrastructure.Database.Commands
{
    public class KeyedCommandConnection(IServiceProvider serviceProvider, string key) : ICommandConnection
    {
        public virtual async Task ExecuteAsync(string name, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            var context = serviceProvider.GetRequiredKeyedService<ICommandContext>(key);
            await context.OpenConnection(async () =>
            {
                var command = new SqlCommand
                {
                    CommandText = name,
                    Connection = context.Connection,
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddRange(parameters?.ToArray());
                await command.ExecuteNonQueryAsync(cancellationToken);
                return parameters?.Where(x => x.Direction == ParameterDirection.Output).FirstOrDefault()?.Value?.ToString();
            });
        }

        public virtual async Task<T?> ExecuteAsync<T>(string name, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            var context = serviceProvider.GetRequiredKeyedService<ICommandContext>(key);
            T? result = await context.OpenConnection(async () =>
            {
                var command = new SqlCommand
                {
                    CommandText = name,
                    Connection = context.Connection,
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddRange(parameters?.Select(x => { x.Value ??= DBNull.Value; return x; }).ToArray());
                return (T)await command.ExecuteScalarAsync(cancellationToken);
            });
            return result;
        }
    }

    public class CommandConnection(ICommandContext context) : ICommandConnection
    {
        public virtual async Task ExecuteAsync(string name, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default)
        {
            await context.OpenConnection(async () =>
            {
                var command = new SqlCommand
                {
                    CommandText = name,
                    Connection = context.Connection,
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddRange(parameters?.ToArray());
                await command.ExecuteNonQueryAsync(cancellationToken);
                return parameters?.Where(x => x.Direction == ParameterDirection.Output).FirstOrDefault()?.Value?.ToString();
            });
        }

        public virtual async Task<T?> ExecuteAsync<T>(string name, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default)
        {
            T? result = await context.OpenConnection(async () =>
            {
                var command = new SqlCommand
                {
                    CommandText = name,
                    Connection = context.Connection,
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddRange(parameters?.Select(x => { x.Value ??= DBNull.Value; return x; }).ToArray());
                return (T)await command.ExecuteScalarAsync(cancellationToken);
            });
            return result;
        }
    }
}
