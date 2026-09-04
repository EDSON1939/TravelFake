using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Clients.Commands.CreateClient;

public record CreateClientCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    long   CountryId) : IRequest<BaseResponse<long>>;
