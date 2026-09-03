using Core.Infrastructure.Database.Commands.Interfaces;
using Microsoft.Data.SqlClient;

namespace Core.Infrastructure.Database.Commands
{
    public abstract class CommandBase
    {
        public virtual string Name => "SQL procedure name is not overriden in inheriting class";

        public virtual IEnumerable<SqlParameter>? Parameters => null;
    }

    public abstract class SqlCommandBase : CommandBase, ICommandBase
    {
        public virtual async Task ExecuteAsync(ICommandConnection connection, CancellationToken cancellationToken = default) => await connection.ExecuteAsync(Name, Parameters, cancellationToken);
    }

    public abstract class SqlCommandBase<T> : CommandBase, ICommandBase<T>
    {
        public virtual async Task<T?> ExecuteAsync(ICommandConnection connection, CancellationToken cancellationToken = default) => await connection.ExecuteAsync<T>(Name, Parameters, cancellationToken);
    }
}
