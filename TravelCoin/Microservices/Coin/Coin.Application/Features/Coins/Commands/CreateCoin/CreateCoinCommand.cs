using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Commands.CreateCoin;

/// <param name="BuyRate">Buy exchange rate as invariant decimal text (e.g. "6.86").</param>
public record CreateCoinCommand(string Name, string Code, string BuyRate)
    : IRequest<BaseResponse<long>>;
