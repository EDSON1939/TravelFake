using Client.Application.Common;
using Client.Domain.Services;
using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Countries.Queries.GetCountries;

/// <summary>
/// No consulta la base de datos: reenvía la petición al microservicio TravelCountry.
/// </summary>
public class GetCountriesQueryHandler(ICountryService countryService)
    : IRequestHandler<GetCountriesQuery, BaseResponse<List<CountryResponse>>>
{
    public async Task<BaseResponse<List<CountryResponse>>> Handle(GetCountriesQuery request, CancellationToken ct)
    {
        var entities = await countryService.GetAll(request.OnlyActive, ct);

        var countries = entities
            .Select(e => new CountryResponse(e.CountryId, e.Name, e.Code, e.IsActive))
            .ToList();

        return BaseResponse<List<CountryResponse>>.Success(countries);
    }
}
