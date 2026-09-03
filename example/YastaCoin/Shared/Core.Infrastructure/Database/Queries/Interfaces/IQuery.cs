namespace Core.Infrastructure.Database.Queries.Interfaces
{
    public interface IQuery
    {
        Task<T?> QuerySqlAsync<T>(IQueryBase<T> sqlQuery, CancellationToken cancellationToken = default);
    }
}
