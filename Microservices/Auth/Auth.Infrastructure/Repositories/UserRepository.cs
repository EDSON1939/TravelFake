using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Persistence.Commands;
using Auth.Infrastructure.Persistence.Queries;
using Core.Infrastructure.Database.Commands.Interfaces;
using Core.Infrastructure.Database.Queries.Interfaces;

namespace Auth.Infrastructure.Repositories;

public class UserRepository(IQuery query, ICommand command) : IUserRepository
{
    public async Task<UserEntity?> GetByUsername(string username, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetUserByUsernameQuery(username), ct);

    public async Task<UserEntity?> GetByClientId(long clientId, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetUserByClientQuery(clientId), ct);

    public async Task<long> Insert(UserEntity entity, CancellationToken ct = default)
        => await command.ExecuteAsync(new InsertUserCommand(entity), ct);

    public async Task<long> RegisterLoginAttempt(
        long userId, bool succeeded, int maxAttempts, int lockMinutes, CancellationToken ct = default)
        => await command.ExecuteAsync(
            new RegisterLoginAttemptCommand(userId, succeeded, maxAttempts, lockMinutes), ct);

    public async Task<long> Delete(long userId, CancellationToken ct = default)
        => await command.ExecuteAsync(new DeleteUserCommand(userId), ct);
}
