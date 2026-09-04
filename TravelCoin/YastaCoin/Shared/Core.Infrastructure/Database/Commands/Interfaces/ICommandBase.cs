namespace Core.Infrastructure.Database.Commands.Interfaces
{
    public interface ICommandBase
    {
        Task ExecuteAsync(ICommandConnection connection, CancellationToken cancellationToken = default);
    }

    public interface ICommandBase<T>
    {
        Task<T?> ExecuteAsync(ICommandConnection connection, CancellationToken cancellationToken = default);
    }
}
