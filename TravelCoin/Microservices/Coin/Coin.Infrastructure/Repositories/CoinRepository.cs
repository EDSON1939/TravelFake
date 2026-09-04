using Coin.Domain.Entities;
using Coin.Domain.Repositories;
using Coin.Infrastructure.Persistence.Commands;
using Coin.Infrastructure.Persistence.Queries;
using Core.Infrastructure.Database.Commands.Interfaces;
using Core.Infrastructure.Database.Queries.Interfaces;

namespace Coin.Infrastructure.Repositories;

public class CoinRepository(IQuery query, ICommand command) : ICoinRepository
{
    public async Task<CoinEntity?> GetById(long coinId, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetCoinByIdQuery(coinId), ct);

    public async Task<IEnumerable<CoinEntity>> GetAll(bool onlyActive, CancellationToken ct = default)
        => await query.QuerySqlAsync(new GetAllCoinsQuery(onlyActive), ct) ?? [];

    public async Task<long> Insert(CoinEntity entity, CancellationToken ct = default)
        => await command.ExecuteAsync(new InsertCoinCommand(entity), ct);

    public async Task<long> Update(CoinEntity entity, CancellationToken ct = default)
        => await command.ExecuteAsync(new UpdateCoinCommand(entity), ct);

    public async Task<long> UpdateStatus(long coinId, bool isActive, CancellationToken ct = default)
        => await command.ExecuteAsync(new UpdateCoinStatusCommand(coinId, isActive), ct);

    public async Task<long> Delete(long coinId, CancellationToken ct = default)
        => await command.ExecuteAsync(new DeleteCoinCommand(coinId), ct);
}
