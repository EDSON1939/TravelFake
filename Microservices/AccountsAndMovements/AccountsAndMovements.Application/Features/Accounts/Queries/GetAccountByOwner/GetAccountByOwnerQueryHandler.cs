using AccountsAndMovements.Application.Common;
using AccountsAndMovements.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountByOwner;

public class GetAccountByOwnerQueryHandler(IAccountRepository repository)
    : IRequestHandler<GetAccountByOwnerQuery, BaseResponse<AccountResponse>>
{
    public async Task<BaseResponse<AccountResponse>> Handle(
        GetAccountByOwnerQuery request, CancellationToken ct)
    {
        var entity = await repository.GetByOwnerAndCoin(
            request.OwnerType.Trim().ToUpper(), request.OwnerId, request.CoinCode.Trim().ToUpper(), ct);

        if (entity is null)
            return BaseResponse<AccountResponse>.Error(
                Domain.Errors.ErrorCode.ACCOUNT_NOT_FOUND,
                Domain.Errors.ErrorMessage.ACCOUNT_NOT_FOUND);

        return BaseResponse<AccountResponse>.Success(entity.ToResponse());
    }
}
