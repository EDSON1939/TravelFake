using Country.Domain.Entities;

namespace Country.Domain.Repositories;

public interface ICountryRepository
{
    Task<CountryEntity?> GetById(long countryId, CancellationToken ct = default);

    Task<IEnumerable<CountryEntity>> GetAll(bool onlyActive, CancellationToken ct = default);

    Task<long> Insert(CountryEntity entity, CancellationToken ct = default);

    Task<long> Update(CountryEntity entity, CancellationToken ct = default);

    Task<long> UpdateStatus(long countryId, bool isActive, CancellationToken ct = default);

    /// <summary>
    /// Baja física. dbo.DELETE_PAIS devuelve -1 cuando el país tiene clientes asociados.
    /// </summary>
    Task<long> Delete(long countryId, CancellationToken ct = default);
}
