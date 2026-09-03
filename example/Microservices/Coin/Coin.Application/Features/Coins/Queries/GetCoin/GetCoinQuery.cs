using Coin.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Queries.GetCoin;

public record GetCoinQuery(string Code) : IRequest<BaseResponse<CoinResponse>>;
