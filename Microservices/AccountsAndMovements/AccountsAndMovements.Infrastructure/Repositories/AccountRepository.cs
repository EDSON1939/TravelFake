using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.Repositories;
using AccountsAndMovements.Infrastructure.Persistence.Commands;
using AccountsAndMovements.Infrastructure.Persistence.Queries;
using Core.Infrastructure.Audit;
using Core.Infrastructure.Database.Commands.Interfaces;
using Core.Infrastructure.Database.Queries.Interfaces;

namespace AccountsAndMovements.Infrastructure.Repositories;

public class AccountRepository(IQuery query, ICommand command, IAuditContext audit) : IAccountRepository
{
    public async Task<AccountEntity?> GetByNumber(string number, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetAccountByNumberQuery(number), ct);

    public async Task<AccountEntity?> GetByOwnerAndCoin(
        string ownerType, long ownerId, string coinCode, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetAccountByOwnerAndCoinQuery(ownerType, ownerId, coinCode), ct);

    public async Task<IEnumerable<AccountEntity>> GetByOwner(
        string ownerType, long ownerId, bool onlyActive, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetAccountsByOwnerQuery(ownerType, ownerId, onlyActive), ct) ?? [];

    public async Task<long> Insert(AccountEntity entity, CancellationToken ct = default)
        => await command.ExecuteAsync(new InsertAccountCommand(entity, audit), ct);
}
