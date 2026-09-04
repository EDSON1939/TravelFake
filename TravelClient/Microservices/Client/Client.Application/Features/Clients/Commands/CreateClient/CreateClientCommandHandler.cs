using Client.Domain.Entities;
using Client.Domain.Repositories;
using Client.Domain.Security;
using Client.Domain.Services;
using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Clients.Commands.CreateClient;

public class CreateClientCommandHandler(
    IClientRepository repository,
    ICountryService countryService,
    IAuthService authService)
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

        var client = new ClientEntity
        {
            FirstName = request.FirstName.Trim(),
            LastName  = request.LastName.Trim(),
            Email     = request.Email.Trim().ToLower(),
            Phone     = request.Phone.Trim(),
            CountryId = country.CountryId
        };

        var id = await repository.Insert(client, ct);

        // travelfake.INSERT_CLIENTE devuelve -1 cuando el país no existe (evita el error 547 de FK_PAIS)
        if (id < 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND,
                Domain.Errors.ErrorMessage.COUNTRY_NOT_FOUND);

        if (id == 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.INSERT_FAILED,
                Domain.Errors.ErrorMessage.INSERT_FAILED);

        client.CustomerId = id;

        // Registrar al cliente también le da acceso al sistema: el usuario es su
        // nombre y la contraseña ese mismo nombre seguido de "123". La credencial
        // la crea el microservicio Auth, único dueño de los usuarios.
        var credential = ClientCredential.FromName(client.FirstName);
        var userId = await authService.CreateUser(client, credential, ct);

        // El cliente ya quedó grabado y el INSERT no se puede deshacer desde aquí:
        // si Auth rechaza la credencial se devuelve éxito avisándolo en el mensaje.
        return BaseResponse<long>.Success(
            id,
            message: string.Format(
                userId is null
                    ? Domain.Errors.ErrorMessage.CLIENT_CREATED_WITHOUT_USER
                    : Domain.Errors.ErrorMessage.CLIENT_CREATED_WITH_USER,
                credential.Username));
    }
}
