using Core.Domain.Models;
using Country.Domain.Entities;
using Country.Domain.Repositories;
using MediatR;

namespace Country.Application.Features.Countries.Commands.UpdateCountry;

public class UpdateCountryCommandHandler(ICountryRepository repository)
    : IRequestHandler<UpdateCountryCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(UpdateCountryCommand request, CancellationToken ct)
    {
        var affected = await repository.Update(new CountryEntity
        {
            CountryId = request.CountryId,
            Name      = request.Name.Trim(),
            Code      = request.Code.Trim().ToUpper(),
            IsActive  = request.IsActive
        }, ct);

        // dbo.UPDATE_PAIS devuelve 0 cuando el PAIS_ID_IT no existe
        if (affected <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND,
                Domain.Errors.ErrorMessage.COUNTRY_NOT_FOUND);

        return BaseResponse<long>.Success(request.CountryId);
    }
}
