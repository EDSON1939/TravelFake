using Microsoft.Data.SqlClient;

namespace Core.Infrastructure.Database.Queries.Interfaces
{
    public interface IQueryContext
    {
        SqlConnection? Connection { get; }

        string ConnectionString { get; }

        Task<T?> OpenConnection<T>(Func<Task<T>> sqlStatement);
    }
}
