using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Commands.UpdateCoinStatus;

public record UpdateCoinStatusCommand(long CoinId, bool IsActive)
    : IRequest<BaseResponse<long>>;
