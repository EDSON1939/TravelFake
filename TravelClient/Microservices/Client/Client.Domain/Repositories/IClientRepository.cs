using Client.Domain.Entities;

namespace Client.Domain.Repositories;

public interface IClientRepository
{
    Task<ClientEntity?> GetById(long customerId, CancellationToken ct = default);

    /// <param name="countryId">null = todos los países.</param>
    Task<IEnumerable<ClientEntity>> GetAll(bool onlyActive, long? countryId, CancellationToken ct = default);

    /// <summary>
    /// travelfake.INSERT_CLIENTE devuelve -1 cuando el CLIENTES_PAIS_ID_IT no existe.
    /// </summary>
    Task<long> Insert(ClientEntity entity, CancellationToken ct = default);

    /// <summary>
    /// travelfake.UPDATE_CLIENTE devuelve -1 cuando el CLIENTES_PAIS_ID_IT no existe.
    /// </summary>
    Task<long> Update(ClientEntity entity, CancellationToken ct = default);

    Task<long> UpdateStatus(long customerId, bool isActive, CancellationToken ct = default);

    Task<long> Delete(long customerId, CancellationToken ct = default);
}
