using Client.Domain.Entities;
using Client.Domain.Repositories;
using Client.Domain.Services;
using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Clients.Commands.UpdateClient;

public class UpdateClientCommandHandler(IClientRepository repository, ICountryService countryService)
    : IRequestHandler<UpdateClientCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(UpdateClientCommand request, CancellationToken ct)
    {
        // Igual que en el alta: TravelCountry es el dueño del catálogo de países.
        var country = await countryService.GetById(request.CountryId, ct);

        if (country is null)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND,
                Domain.Errors.ErrorMessage.COUNTRY_NOT_FOUND);

        if (!country.IsActive)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COUNTRY_INACTIVE,
                Domain.Errors.ErrorMessage.COUNTRY_INACTIVE);

        var affected = await repository.Update(new ClientEntity
        {
            CustomerId = request.CustomerId,
            FirstName  = request.FirstName.Trim(),
            LastName   = request.LastName.Trim(),
            Email      = request.Email.Trim().ToLower(),
            Phone      = request.Phone.Trim(),
            CountryId  = country.CountryId,
            IsActive   = request.IsActive
        }, ct);

        // -1 = el país indicado no existe
        if (affected < 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND,
                Domain.Errors.ErrorMessage.COUNTRY_NOT_FOUND);

        // 0 = el CLIENTES_ID_IT no existe
        if (affected == 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.CLIENT_NOT_FOUND,
                Domain.Errors.ErrorMessage.CLIENT_NOT_FOUND);

        return BaseResponse<long>.Success(request.CustomerId);
    }
}
