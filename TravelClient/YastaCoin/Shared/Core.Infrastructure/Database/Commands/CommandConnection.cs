using Core.Infrastructure.Database.Commands.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Globalization;

namespace Core.Infrastructure.Database.Commands
{
    /// <summary>
    /// Convierte el escalar devuelto por ExecuteScalarAsync al tipo esperado por el comando.
    /// Un cast directo (T)valor falla con InvalidCastException cuando SQL Server devuelve
    /// otro tipo numerico al declarado (p. ej. SCOPE_IDENTITY() es NUMERIC(38,0) => decimal,
    /// y @@ROWCOUNT es INT => int), o cuando el SP no devuelve filas (null / DBNull).
    /// </summary>
    internal static class ScalarConverter
    {
        public static T? To<T>(object? value)
        {
            if (value is null || value is DBNull)
            {
                return default;
            }

            if (value is T typedValue)
            {
                return typedValue;
            }

            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            return (T)Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
    }

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
                return ScalarConverter.To<T>(await command.ExecuteScalarAsync(cancellationToken));
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
                return ScalarConverter.To<T>(await command.ExecuteScalarAsync(cancellationToken));
            });
            return result;
        }
    }
}
