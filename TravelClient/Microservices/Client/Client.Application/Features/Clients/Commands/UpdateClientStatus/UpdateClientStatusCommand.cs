using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Clients.Commands.UpdateClientStatus;

public record UpdateClientStatusCommand(long CustomerId, bool IsActive)
    : IRequest<BaseResponse<long>>;
