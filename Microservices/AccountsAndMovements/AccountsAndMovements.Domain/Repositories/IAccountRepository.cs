using AccountsAndMovements.Domain.Entities;

namespace AccountsAndMovements.Domain.Repositories;

public interface IAccountRepository
{
    Task<AccountEntity?> GetByNumber(string number, CancellationToken ct = default);

    /// <summary>
    /// Resuelve (titular, moneda) -> cuenta. Es el paso que traduce "cliente 7 que
    /// paga en USD" al numero de cuenta que necesita el SP para mover saldo.
    /// </summary>
    Task<AccountEntity?> GetByOwnerAndCoin(
        string ownerType, long ownerId, string coinCode, CancellationToken ct = default);

    Task<IEnumerable<AccountEntity>> GetByOwner(
        string ownerType, long ownerId, bool onlyActive, CancellationToken ct = default);

    Task<long> Insert(AccountEntity entity, CancellationToken ct = default);
}
