namespace Core.Infrastructure.Database.Queries.Interfaces
{
    public interface IQueryBase<T>
    {
        Task<T?> ExecuteAsync(IQueryConnection connection, CancellationToken cancellationToken = default);
    }
}
