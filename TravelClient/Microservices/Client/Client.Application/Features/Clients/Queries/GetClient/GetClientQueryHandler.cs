using Client.Application.Common;
using Client.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Clients.Queries.GetClient;

public class GetClientQueryHandler(IClientRepository repository)
    : IRequestHandler<GetClientQuery, BaseResponse<ClientResponse>>
{
    public async Task<BaseResponse<ClientResponse>> Handle(GetClientQuery request, CancellationToken ct)
    {
        var entity = await repository.GetById(request.CustomerId, ct);

        if (entity is null)
            return BaseResponse<ClientResponse>.Error(
                Domain.Errors.ErrorCode.CLIENT_NOT_FOUND,
                Domain.Errors.ErrorMessage.CLIENT_NOT_FOUND);

        return BaseResponse<ClientResponse>.Success(new ClientResponse(
            entity.CustomerId, entity.FirstName, entity.LastName,
            entity.Email, entity.Phone,
            entity.CountryId, entity.CountryName, entity.CountryCode,
            entity.IsActive, entity.CreatedAt, entity.UpdatedAt));
    }
}
