using Coin.Application.Common;
using Coin.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Queries.GetCoin;

public class GetCoinQueryHandler(ICoinRepository repository)
    : IRequestHandler<GetCoinQuery, BaseResponse<CoinResponse>>
{
    public async Task<BaseResponse<CoinResponse>> Handle(GetCoinQuery request, CancellationToken ct)
    {
        var entity = await repository.GetById(request.CoinId, ct);

        if (entity is null)
            return BaseResponse<CoinResponse>.Error(
                Domain.Errors.ErrorCode.COIN_NOT_FOUND,
                Domain.Errors.ErrorMessage.COIN_NOT_FOUND);

        return BaseResponse<CoinResponse>.Success(new CoinResponse(
            entity.CoinId, entity.Name, entity.Code, entity.BuyRate,
            entity.IsActive, entity.CreatedAt, entity.UpdatedAt));
    }
}
