using Core.Domain.Models;
using Country.Domain.Repositories;
using MediatR;

namespace Country.Application.Features.Countries.Commands.DeleteCountry;

public class DeleteCountryCommandHandler(ICountryRepository repository)
    : IRequestHandler<DeleteCountryCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(DeleteCountryCommand request, CancellationToken ct)
    {
        var affected = await repository.Delete(request.CountryId, ct);

        // dbo.DELETE_PAIS: -1 = el país tiene clientes asociados (FK_PAIS)
        if (affected < 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COUNTRY_IN_USE,
                Domain.Errors.ErrorMessage.COUNTRY_IN_USE);

        // 0 = el PAIS_ID_IT no existe
        if (affected == 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND,
                Domain.Errors.ErrorMessage.COUNTRY_NOT_FOUND);

        return BaseResponse<long>.Success(request.CountryId);
    }
}
