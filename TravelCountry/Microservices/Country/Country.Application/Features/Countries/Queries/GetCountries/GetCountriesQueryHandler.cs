using Core.Domain.Models;
using Country.Application.Common;
using Country.Domain.Repositories;
using MediatR;

namespace Country.Application.Features.Countries.Queries.GetCountries;

public class GetCountriesQueryHandler(ICountryRepository repository)
    : IRequestHandler<GetCountriesQuery, BaseResponse<List<CountryResponse>>>
{
    public async Task<BaseResponse<List<CountryResponse>>> Handle(GetCountriesQuery request, CancellationToken ct)
    {
        var entities = await repository.GetAll(request.OnlyActive, ct);

        var countries = entities
            .Select(e => new CountryResponse(
                e.CountryId, e.Name, e.Code, e.IsActive, e.CreatedAt, e.UpdatedAt))
            .ToList();

        return BaseResponse<List<CountryResponse>>.Success(countries);
    }
}
