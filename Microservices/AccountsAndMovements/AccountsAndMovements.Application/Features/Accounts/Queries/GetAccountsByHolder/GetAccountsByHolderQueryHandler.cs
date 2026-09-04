using AccountsAndMovements.Application.Common;
using AccountsAndMovements.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountsByHolder;

public class GetAccountsByHolderQueryHandler(IAccountRepository repository)
    : IRequestHandler<GetAccountsByHolderQuery, BaseResponse<List<AccountResponse>>>
{
    public async Task<BaseResponse<List<AccountResponse>>> Handle(
        GetAccountsByHolderQuery request, CancellationToken ct)
    {
        var entities = await repository.GetByHolder(
            request.AccountType.Trim().ToUpper(), request.HolderId, request.OnlyActive, ct);

        return BaseResponse<List<AccountResponse>>.Success(
            entities.Select(e => e.ToResponse()).ToList());
    }
}
