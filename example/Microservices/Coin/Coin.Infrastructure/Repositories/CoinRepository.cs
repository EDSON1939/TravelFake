using Coin.Domain.Entities;
using Coin.Domain.Repositories;
using Coin.Infrastructure.Persistence.Commands;
using Coin.Infrastructure.Persistence.Queries;
using Core.Infrastructure.Database.Commands.Interfaces;
using Core.Infrastructure.Database.Queries.Interfaces;

namespace Coin.Infrastructure.Repositories;

public class CoinRepository(IQuery query, ICommand command) : ICoinRepository
{
    public async Task<CoinEntity?> GetByCode(string code, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetCoinByCodeQuery(code), ct);

    public async Task<IEnumerable<CoinEntity>> GetAll(bool onlyActive, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetAllCoinsQuery(onlyActive), ct) ?? [];

    public async Task<long> Insert(CoinEntity entity, CancellationToken ct = default)
        => await command.ExecuteAsync(new InsertCoinCommand(entity), ct);

    public async Task<long> UpdateStatus(long coinId, bool isActive, CancellationToken ct = default)
        => await command.ExecuteAsync(new UpdateCoinStatusCommand(coinId, isActive), ct);
}
