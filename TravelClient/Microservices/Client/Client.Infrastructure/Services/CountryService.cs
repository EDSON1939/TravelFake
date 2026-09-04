using System.Globalization;
using Client.Domain.Entities;
using Client.Domain.Services;
using Client.Infrastructure.Grpc;
using Core.Domain.Errors;

namespace Client.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="ICountryService"/> sobre el canal gRPC hacia TravelCountry.
/// El canal se configura en <see cref="Extensions.DependencyInjection"/> con la sección
/// Connections:Country de appsettings.json.
/// </summary>
public class CountryService(Country.CountryClient client) : ICountryService
{
    public async Task<CountryEntity?> GetById(long countryId, CancellationToken ct = default)
    {
        var response = await client.GetCountryAsync(
            new GetCountryRequestPb { CountryId = countryId },
            cancellationToken: ct);

        // TravelCountry responde COUNTRY_NOT_FOUND (no una excepción) cuando el país no existe.
        if (response.StatusCode != ErrorCode.SUC000 || response.Data is null)
            return null;

        return Map(response.Data);
    }

    public async Task<IEnumerable<CountryEntity>> GetAll(bool onlyActive, CancellationToken ct = default)
    {
        var response = await client.GetCountriesAsync(
            new GetCountriesRequestPb { OnlyActive = onlyActive },
            cancellationToken: ct);

        if (response.StatusCode != ErrorCode.SUC000)
            return [];

        return response.Data.Select(Map).ToList();
    }

    private static CountryEntity Map(CountryDataPb data) => new()
    {
        CountryId = data.CountryId,
        Name      = data.Name,
        Code      = data.Code,
        IsActive  = data.IsActive,
        CreatedAt = ParseDate(data.CreatedAt) ?? default,
        UpdatedAt = ParseDate(data.UpdatedAt)
    };

    // TravelCountry serializa las fechas con ToString("o") y envía cadena vacía cuando son null.
    private static DateTime? ParseDate(string value) =>
        DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date)
            ? date
            : null;
}
