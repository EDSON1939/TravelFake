using Coin.Application.Common;
using Coin.Domain.Entities;
using Coin.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Commands.UpdateCoin;

public class UpdateCoinCommandHandler(ICoinRepository repository)
    : IRequestHandler<UpdateCoinCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(UpdateCoinCommand request, CancellationToken ct)
    {
        var affected = await repository.Update(new CoinEntity
        {
            CoinId   = request.CoinId,
            Name     = request.Name.Trim(),
            Code     = request.Code.Trim().ToUpper(),
            BuyRate  = MoneyRules.Parse(request.BuyRate),
            IsActive = request.IsActive
        }, ct);

        // travelfake.UPDATE_MONEDA returns 0 when MONEDAS_ID_IT does not exist
        if (affected <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COIN_NOT_FOUND,
                Domain.Errors.ErrorMessage.COIN_NOT_FOUND);

        return BaseResponse<long>.Success(request.CoinId);
    }
}
