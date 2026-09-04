using AutoMapper;
using Coin.Api.Grpc;
using Coin.Application.Features.Coins.Commands.CreateCoin;
using Coin.Application.Features.Coins.Commands.UpdateCoin;
using Coin.Application.Features.Coins.Queries.GetCoin;
using Coin.Application.Features.Coins.Queries.GetCoins;
using Grpc.Core;
using MediatR;

namespace Coin.Api.Services;

public class CoinService(ISender sender, IMapper mapper) : Coin.Api.Grpc.Coin.CoinBase
{
    public override async Task<GetCoinBaseResponsePb> GetCoin(
        GetCoinRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetCoinQuery(request.Code),
            context.CancellationToken);

        return mapper.Map<GetCoinBaseResponsePb>(result);
    }

    public override async Task<GetCoinsBaseResponsePb> GetCoins(
        GetCoinsRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetCoinsQuery(request.OnlyActive),
            context.CancellationToken);

        return mapper.Map<GetCoinsBaseResponsePb>(result);
    }

    public override async Task<CoinMutationBaseResponsePb> CreateCoin(
        CreateCoinRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new CreateCoinCommand(request.Name, request.Code, request.Symbol),
            context.CancellationToken);

        return mapper.Map<CoinMutationBaseResponsePb>(result);
    }

    public override async Task<CoinMutationBaseResponsePb> UpdateCoin(
        UpdateCoinRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new UpdateCoinCommand(request.CoinId, request.IsActive),
            context.CancellationToken);

        return mapper.Map<CoinMutationBaseResponsePb>(result);
    }
}
