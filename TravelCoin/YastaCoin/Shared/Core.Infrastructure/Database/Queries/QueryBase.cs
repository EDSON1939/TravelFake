using Dapper;
using System.Data;
using Core.Infrastructure.Database.Queries.Interfaces;

namespace Core.Infrastructure.Database.Queries
{
    public abstract class QueryBase
    {
        public virtual string SqlStatement => "SQL Statement is not overriden in inheriting class";

        public virtual string? Procedure => null;

        public virtual DynamicParameters? Parameters => null;

        protected CommandType ExecutionType => string.IsNullOrEmpty(Procedure) ? CommandType.Text : CommandType.StoredProcedure;
    }

    public abstract class QuerySingleBase<T> : QueryBase, IQueryBase<T>
    {
        public virtual async Task<T?> ExecuteAsync(IQueryConnection connection, CancellationToken cancellationToken = default)
            => await connection.QueryFirstOrDefaultAsync<T>(Procedure ?? SqlStatement, Parameters, ExecutionType, cancellationToken);
    }

    public abstract class QueryMultipleBase<T> : QueryBase, IQueryBase<IEnumerable<T>>
    {
        public virtual async Task<IEnumerable<T>?> ExecuteAsync(IQueryConnection connection, CancellationToken cancellationToken = default)
                  => await connection.QueryAsync<T>(Procedure ?? SqlStatement, Parameters, ExecutionType, cancellationToken);
    }
}
