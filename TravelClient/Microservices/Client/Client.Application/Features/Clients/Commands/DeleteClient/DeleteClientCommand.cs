using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Clients.Commands.DeleteClient;

public record DeleteClientCommand(long CustomerId) : IRequest<BaseResponse<long>>;
