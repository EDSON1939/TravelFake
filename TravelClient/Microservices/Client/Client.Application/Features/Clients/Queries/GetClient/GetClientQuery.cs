using Client.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Clients.Queries.GetClient;

public record GetClientQuery(long CustomerId) : IRequest<BaseResponse<ClientResponse>>;
