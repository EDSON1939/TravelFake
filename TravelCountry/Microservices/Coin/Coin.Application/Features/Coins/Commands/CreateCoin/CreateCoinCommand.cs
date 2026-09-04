using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Commands.CreateCoin;

public record CreateCoinCommand(string Name, string Code, string Symbol)
    : IRequest<BaseResponse<long>>;
