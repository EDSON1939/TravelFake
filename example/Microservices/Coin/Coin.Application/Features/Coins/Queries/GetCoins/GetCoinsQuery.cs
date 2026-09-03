using Coin.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Queries.GetCoins;

public record GetCoinsQuery(bool OnlyActive = true) : IRequest<BaseResponse<List<CoinResponse>>>;
