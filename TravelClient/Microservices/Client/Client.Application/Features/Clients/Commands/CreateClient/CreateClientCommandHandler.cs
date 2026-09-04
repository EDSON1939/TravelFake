using Client.Domain.Entities;
using Client.Domain.Repositories;
using Client.Domain.Services;
using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Clients.Commands.CreateClient;

public class CreateClientCommandHandler(IClientRepository repository, ICountryService countryService)
    : IRequestHandler<CreateClientCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(CreateClientCommand request, CancellationToken ct)
    {
        // El país lo administra el microservicio TravelCountry: se consulta antes de insertar
        // para devolver un error de negocio en lugar de chocar contra la FK_PAIS.
        var country = await countryService.GetById(request.CountryId, ct);

        if (country is null)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND,
                Domain.Errors.ErrorMessage.COUNTRY_NOT_FOUND);

        if (!country.IsActive)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COUNTRY_INACTIVE,
                Domain.Errors.ErrorMessage.COUNTRY_INACTIVE);

        var id = await repository.Insert(new ClientEntity
        {
            FirstName = request.FirstName.Trim(),
            LastName  = request.LastName.Trim(),
            Email     = request.Email.Trim().ToLower(),
            Phone     = request.Phone.Trim(),
            CountryId = country.CountryId
        }, ct);

        // travelfake.INSERT_CLIENTE devuelve -1 cuando el país no existe (evita el error 547 de FK_PAIS)
        if (id < 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND,
                Domain.Errors.ErrorMessage.COUNTRY_NOT_FOUND);

        if (id == 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.INSERT_FAILED,
                Domain.Errors.ErrorMessage.INSERT_FAILED);

        return BaseResponse<long>.Success(id);
    }
}
