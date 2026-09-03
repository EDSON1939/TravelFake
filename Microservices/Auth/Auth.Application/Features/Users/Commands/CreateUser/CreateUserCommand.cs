using Core.Domain.Models;
using MediatR;

namespace Auth.Application.Features.Users.Commands.CreateUser;

/// <param name="Document">
/// Documento del cliente del negocio al que pertenece el usuario. Vacío para
/// operadores internos, que no representan a ningún cliente.
/// </param>
public record CreateUserCommand(
    string  Username,
    string  Password,
    string  Document,
    string  FullName,
    string  Role) : IRequest<BaseResponse<long>>;
