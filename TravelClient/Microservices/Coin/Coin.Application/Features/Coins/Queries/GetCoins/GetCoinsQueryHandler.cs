using Coin.Application.Common;
using Coin.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Queries.GetCoins;

public class GetCoinsQueryHandler(ICoinRepository repository)
    : IRequestHandler<GetCoinsQuery, BaseResponse<List<CoinResponse>>>
{
    public async Task<BaseResponse<List<CoinResponse>>> Handle(GetCoinsQuery request, CancellationToken ct)
    {
        var entities = await repository.GetAll(request.OnlyActive, ct);

        var coins = entities
            .Select(e => new CoinResponse(e.CoinId, e.Name, e.Code, e.Symbol, e.IsActive, e.CreatedAt))
            .ToList();

        return BaseResponse<List<CoinResponse>>.Success(coins);
    }
}
