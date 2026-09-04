using Coin.Domain.Entities;

namespace Coin.Domain.Repositories;

public interface ICoinRepository
{
    Task<CoinEntity?> GetByCode(string code, CancellationToken ct = default);
    Task<IEnumerable<CoinEntity>> GetAll(bool onlyActive, CancellationToken ct = default);
    Task<long> Insert(CoinEntity entity, CancellationToken ct = default);
    Task<long> UpdateStatus(long coinId, bool isActive, CancellationToken ct = default);
}
