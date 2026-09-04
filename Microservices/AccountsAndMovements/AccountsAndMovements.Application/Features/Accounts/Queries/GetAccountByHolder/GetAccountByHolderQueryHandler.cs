using AccountsAndMovements.Application.Common;
using AccountsAndMovements.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountByHolder;

public class GetAccountByHolderQueryHandler(IAccountRepository repository)
    : IRequestHandler<GetAccountByHolderQuery, BaseResponse<AccountResponse>>
{
    public async Task<BaseResponse<AccountResponse>> Handle(
        GetAccountByHolderQuery request, CancellationToken ct)
    {
        var entity = await repository.GetByHolderAndCoin(
            request.AccountType.Trim().ToUpper(), request.HolderId, request.CoinCode.Trim().ToUpper(), ct);

        if (entity is null)
            return BaseResponse<AccountResponse>.Error(
                Domain.Errors.ErrorCode.ACCOUNT_NOT_FOUND,
                Domain.Errors.ErrorMessage.ACCOUNT_NOT_FOUND);

        return BaseResponse<AccountResponse>.Success(entity.ToResponse());
    }
}
