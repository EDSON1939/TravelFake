using Core.Infrastructure.Database.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Database.Commands
{
    public class KeyedCommand(IServiceProvider serviceProvider, string key) : ICommand
    {
        public async Task ExecuteAsync(ICommandBase command, CancellationToken cancellationToken = default)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            var connection = serviceProvider.GetRequiredKeyedService<ICommandConnection>(key);
            await command.ExecuteAsync(connection, cancellationToken);
        }

        public async Task<T?> ExecuteAsync<T>(ICommandBase<T> command, CancellationToken cancellationToken = default)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            var connection = serviceProvider.GetRequiredKeyedService<ICommandConnection>(key);
            return await command.ExecuteAsync(connection, cancellationToken);
        }
    }

    public class Command(ICommandConnection connection) : ICommand
    {
        public async Task ExecuteAsync(ICommandBase command, CancellationToken cancellationToken = default) => await command.ExecuteAsync(connection, cancellationToken);

        public async Task<T?> ExecuteAsync<T>(ICommandBase<T> command, CancellationToken cancellationToken = default) => await command.ExecuteAsync(connection, cancellationToken);
    }
}
