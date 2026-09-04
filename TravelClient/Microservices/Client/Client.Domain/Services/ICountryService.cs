using Client.Domain.Entities;

namespace Client.Domain.Services;

/// <summary>
/// Puerto hacia el microservicio TravelCountry.
/// La implementación (gRPC) vive en Client.Infrastructure.
/// </summary>
public interface ICountryService
{
    /// <returns>null cuando el país no existe en TravelCountry.</returns>
    Task<CountryEntity?> GetById(long countryId, CancellationToken ct = default);

    Task<IEnumerable<CountryEntity>> GetAll(bool onlyActive, CancellationToken ct = default);
}
