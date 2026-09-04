using Coin.Application.Common;
using Coin.Domain.Entities;
using Coin.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Queries.ConvertToBoliviano;

public class ConvertToBolivianoQueryHandler(ICoinRepository repository)
    : IRequestHandler<ConvertToBolivianoQuery, BaseResponse<ConversionResponse>>
{
    public async Task<BaseResponse<ConversionResponse>> Handle(ConvertToBolivianoQuery request, CancellationToken ct)
    {
        var coin = await repository.GetById(request.CoinId, ct);

        if (coin is null)
            return BaseResponse<ConversionResponse>.Error(
                Domain.Errors.ErrorCode.COIN_NOT_FOUND,
                Domain.Errors.ErrorMessage.COIN_NOT_FOUND);

        // A deactivated coin keeps its last exchange rate, but that value is no
        // longer current: quoting with it is not allowed.
        if (!coin.IsActive)
            return BaseResponse<ConversionResponse>.Error(
                Domain.Errors.ErrorCode.COIN_INACTIVE,
                Domain.Errors.ErrorMessage.COIN_INACTIVE);

        if (coin.BuyRate <= 0)
            return BaseResponse<ConversionResponse>.Error(
                Domain.Errors.ErrorCode.INVALID_EXCHANGE_RATE,
                Domain.Errors.ErrorMessage.INVALID_EXCHANGE_RATE);

        var amount     = MoneyRules.Parse(request.Amount);
        var amountInBs = coin.ToBoliviano(amount);

        return BaseResponse<ConversionResponse>.Success(new ConversionResponse(
            coin.CoinId,
            coin.Code,
            coin.Name,
            coin.BuyRate,
            amount,
            amountInBs,
            Money.BolivianoCode,
            Money.BolivianoName));
    }
}
