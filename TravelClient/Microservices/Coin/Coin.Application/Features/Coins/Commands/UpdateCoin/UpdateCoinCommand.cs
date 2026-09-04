using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Commands.UpdateCoin;

public record UpdateCoinCommand(long CoinId, bool IsActive)
    : IRequest<BaseResponse<long>>;
