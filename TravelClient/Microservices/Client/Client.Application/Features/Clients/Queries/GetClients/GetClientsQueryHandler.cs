using Client.Application.Common;
using Client.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Clients.Queries.GetClients;

public class GetClientsQueryHandler(IClientRepository repository)
    : IRequestHandler<GetClientsQuery, BaseResponse<List<ClientResponse>>>
{
    public async Task<BaseResponse<List<ClientResponse>>> Handle(GetClientsQuery request, CancellationToken ct)
    {
        var entities = await repository.GetAll(request.OnlyActive, request.CountryId, ct);

        var clients = entities
            .Select(e => new ClientResponse(
                e.CustomerId, e.FirstName, e.LastName,
                e.Email, e.Phone,
                e.CountryId, e.CountryName, e.CountryCode,
                e.IsActive, e.CreatedAt, e.UpdatedAt))
            .ToList();

        return BaseResponse<List<ClientResponse>>.Success(clients);
    }
}
