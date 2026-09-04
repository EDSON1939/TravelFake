namespace Core.Infrastructure.Database.Commands.Interfaces
{
    public interface ICommand
    {
        Task ExecuteAsync(ICommandBase command, CancellationToken cancellationToken = default);

        Task<T?> ExecuteAsync<T>(ICommandBase<T> command, CancellationToken cancellationToken = default);
    }
}
