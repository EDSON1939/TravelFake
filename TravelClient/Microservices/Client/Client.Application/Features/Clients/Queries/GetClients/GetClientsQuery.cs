using Client.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Clients.Queries.GetClients;

/// <param name="CountryId">null = todos los países.</param>
public record GetClientsQuery(bool OnlyActive = true, long? CountryId = null)
    : IRequest<BaseResponse<List<ClientResponse>>>;
