using Microsoft.Data.SqlClient;

namespace Core.Infrastructure.Database.Commands.Interfaces
{
    public interface ICommandConnection
    {
        Task ExecuteAsync(string name, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default);

        Task<T?> ExecuteAsync<T>(string name, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default);
    }
}
