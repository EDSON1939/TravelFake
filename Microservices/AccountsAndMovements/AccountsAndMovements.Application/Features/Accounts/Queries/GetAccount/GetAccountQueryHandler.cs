using AccountsAndMovements.Application.Common;
using AccountsAndMovements.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccount;

public class GetAccountQueryHandler(IAccountRepository repository)
    : IRequestHandler<GetAccountQuery, BaseResponse<AccountResponse>>
{
    public async Task<BaseResponse<AccountResponse>> Handle(GetAccountQuery request, CancellationToken ct)
    {
        var entity = await repository.GetByNumber(request.Number.Trim().ToUpper(), ct);

        if (entity is null)
            return BaseResponse<AccountResponse>.Error(
                Domain.Errors.ErrorCode.ACCOUNT_NOT_FOUND,
                Domain.Errors.ErrorMessage.ACCOUNT_NOT_FOUND);

        return BaseResponse<AccountResponse>.Success(entity.ToResponse());
    }
}
