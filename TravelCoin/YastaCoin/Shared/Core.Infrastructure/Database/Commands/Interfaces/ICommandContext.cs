using Microsoft.Data.SqlClient;

namespace Core.Infrastructure.Database.Commands.Interfaces
{
    public interface ICommandContext
    {
        SqlConnection? Connection { get; }

        string ConnectionString { get; }

        Task<T?> OpenConnection<T>(Func<Task<T>> command);
    }
}
