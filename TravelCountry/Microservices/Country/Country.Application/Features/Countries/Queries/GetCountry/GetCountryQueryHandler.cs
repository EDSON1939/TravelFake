using Core.Domain.Models;
using Country.Application.Common;
using Country.Domain.Repositories;
using MediatR;

namespace Country.Application.Features.Countries.Queries.GetCountry;

public class GetCountryQueryHandler(ICountryRepository repository)
    : IRequestHandler<GetCountryQuery, BaseResponse<CountryResponse>>
{
    public async Task<BaseResponse<CountryResponse>> Handle(GetCountryQuery request, CancellationToken ct)
    {
        var entity = await repository.GetById(request.CountryId, ct);

        if (entity is null)
            return BaseResponse<CountryResponse>.Error(
                Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND,
                Domain.Errors.ErrorMessage.COUNTRY_NOT_FOUND);

        return BaseResponse<CountryResponse>.Success(new CountryResponse(
            entity.CountryId, entity.Name, entity.Code,
            entity.IsActive, entity.CreatedAt, entity.UpdatedAt));
    }
}
