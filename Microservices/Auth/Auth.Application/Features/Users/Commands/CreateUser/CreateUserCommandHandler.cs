using Auth.Domain.Entities;
using Auth.Domain.ExternalServices;
using Auth.Domain.Repositories;
using Auth.Domain.Security;
using Core.Domain.Models;
using MediatR;

namespace Auth.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler(
    IUserRepository repository,
    IClientService clientService,
    IPasswordHasher passwordHasher)
    : IRequestHandler<CreateUserCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var username = request.Username.Trim().ToLowerInvariant();

        var existing = await repository.GetByUsername(username, ct);
        if (existing is not null)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.USER_DUPLICATE,
                Domain.Errors.ErrorMessage.USER_DUPLICATE);

        // El vínculo con el cliente se resuelve contra el MS de Client, igual que
        // hacen Account y Transfer. Guardar un ClientId sin verificarlo dejaría
        // usuarios apuntando a clientes inexistentes, y commerce.USUARIO no
        // puede tener FK porque CLIENTE la gobierna otro microservicio.
        long? clientId = null;

        if (!string.IsNullOrWhiteSpace(request.Document))
        {
            var client = await clientService.GetByDocument(request.Document.Trim().ToUpper(), ct);

            if (client is null)
                return BaseResponse<long>.Error(
                    Domain.Errors.ErrorCode.CLIENT_NOT_FOUND,
                    Domain.Errors.ErrorMessage.CLIENT_NOT_FOUND);

            if (!client.IsActive)
                return BaseResponse<long>.Error(
                    Domain.Errors.ErrorCode.CLIENT_INACTIVE,
                    Domain.Errors.ErrorMessage.CLIENT_INACTIVE);

            // Un cliente tiene una sola credencial: si ya tiene usuario, crear
            // otro daría dos accesos a la misma cuenta bancaria.
            var existingUser = await repository.GetByClientId(client.ClientId, ct);
            if (existingUser is not null)
                return BaseResponse<long>.Error(
                    Domain.Errors.ErrorCode.CLIENT_ALREADY_HAS_USER,
                    Domain.Errors.ErrorMessage.CLIENT_ALREADY_HAS_USER);

            clientId = client.ClientId;
        }

        // La contraseña se hashea aquí: en claro no llega nunca a la base.
        var id = await repository.Insert(new UserEntity
        {
            Username     = username,
            PasswordHash = passwordHasher.Hash(request.Password),
            ClientId     = clientId,
            FullName     = request.FullName.Trim(),
            Role         = request.Role
        }, ct);

        if (id <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.INSERT_FAILED,
                Domain.Errors.ErrorMessage.INSERT_FAILED);

        return BaseResponse<long>.Success(id);
    }
}
