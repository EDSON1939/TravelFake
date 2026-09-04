using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Commands.DeleteCoin;

public record DeleteCoinCommand(long CoinId) : IRequest<BaseResponse<long>>;
