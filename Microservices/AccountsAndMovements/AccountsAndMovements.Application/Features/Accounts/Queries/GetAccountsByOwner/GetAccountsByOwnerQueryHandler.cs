using AccountsAndMovements.Application.Common;
using AccountsAndMovements.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountsByOwner;

public class GetAccountsByOwnerQueryHandler(IAccountRepository repository)
    : IRequestHandler<GetAccountsByOwnerQuery, BaseResponse<List<AccountResponse>>>
{
    public async Task<BaseResponse<List<AccountResponse>>> Handle(
        GetAccountsByOwnerQuery request, CancellationToken ct)
    {
        var entities = await repository.GetByOwner(
            request.OwnerType.Trim().ToUpper(), request.OwnerId, request.OnlyActive, ct);

        return BaseResponse<List<AccountResponse>>.Success(
            entities.Select(e => e.ToResponse()).ToList());
    }
}
