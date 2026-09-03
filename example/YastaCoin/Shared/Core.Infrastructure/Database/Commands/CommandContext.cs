using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Core.Infrastructure.Database.Commands.Interfaces;

namespace Core.Infrastructure.Database.Commands
{
    public sealed class CommandContext : ICommandContext
    {
        private SqlConnection? connection;

        public string ConnectionString { get; }

        public TimeSpan ExecutionTime { get; private set; }

        public CommandContext(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "SQL Database Context object did not receive SQL Connection string during its construction.");
            }
            ConnectionString = connectionString;
        }

        public SqlConnection? Connection
        {
            get
            {
                EnsureOpenConnection();
                return connection;
            }
        }

        private void EnsureOpenConnection()
        {
            connection ??= new SqlConnection(ConnectionString);
            if (connection.State == ConnectionState.Closed)
            {
                if (string.IsNullOrEmpty(connection.ConnectionString))
                {
                    connection.ConnectionString = ConnectionString;
                }
                connection.Open();
            }
        }

        public T? OpenConnection<T>(Func<T> command)
        {
            try
            {
                EnsureOpenConnection();
                var counter = Stopwatch.StartNew();
                T result = command();
                counter.Stop();
                ExecutionTime = counter.Elapsed;
                return result;
            }
            catch
            {
                EnsureOpenConnection();
                //TODO: Review throw
                throw;
            }
            finally
            {
                connection?.Close();
                SqlConnection.ClearAllPools();
            }
        }

        public async Task<T?> OpenConnection<T>(Func<Task<T>> command)
        {
            try
            {
                EnsureOpenConnection();
                var counter = Stopwatch.StartNew();
                T result = await command();
                counter.Stop();
                ExecutionTime = counter.Elapsed;
                return result;
            }
            catch
            {
                EnsureOpenConnection();
                //TODO: Review throw
                throw;
            }
            finally
            {
                connection?.Close();
                SqlConnection.ClearAllPools();
            }
        }
    }
}
