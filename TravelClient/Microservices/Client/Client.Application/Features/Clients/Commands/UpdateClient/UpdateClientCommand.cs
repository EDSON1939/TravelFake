using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Clients.Commands.UpdateClient;

public record UpdateClientCommand(
    long   CustomerId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    long   CountryId,
    bool   IsActive) : IRequest<BaseResponse<long>>;
