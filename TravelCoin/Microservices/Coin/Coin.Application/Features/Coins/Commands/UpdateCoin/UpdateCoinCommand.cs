using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Commands.UpdateCoin;

/// <param name="BuyRate">Buy exchange rate as invariant decimal text (e.g. "6.86").</param>
public record UpdateCoinCommand(long CoinId, string Name, string Code, string BuyRate, bool IsActive)
    : IRequest<BaseResponse<long>>;
