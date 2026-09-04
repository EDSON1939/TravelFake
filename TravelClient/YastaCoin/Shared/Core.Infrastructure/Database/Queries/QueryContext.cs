using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Core.Infrastructure.Database.Queries.Interfaces;

namespace Core.Infrastructure.Database.Queries
{
    public sealed class QueryContext : IQueryContext
    {
        private SqlConnection? connection;

        public string ConnectionString { get; }

        public QueryContext(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "SQL Database Context object did not receive SQL connection.");
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

        public async Task<T?> OpenConnection<T>(Func<Task<T>> sqlStatement)
        {
            try
            {
                EnsureOpenConnection();
                var counter = Stopwatch.StartNew();
                T queryResult = await sqlStatement();
                counter.Stop();
                return queryResult;
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
