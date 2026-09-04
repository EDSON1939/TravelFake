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

    public async Task<AccountEntity?> GetByHolderAndCoin(
        string accountType, long holderId, string coinCode, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetAccountByHolderAndCoinQuery(accountType, holderId, coinCode), ct);

    public async Task<IEnumerable<AccountEntity>> GetByHolder(
        string accountType, long holderId, bool onlyActive, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetAccountsByHolderQuery(accountType, holderId, onlyActive), ct) ?? [];

    public async Task<long> Insert(AccountEntity entity, CancellationToken ct = default)
        => await command.ExecuteAsync(new InsertAccountCommand(entity, audit), ct);
}
