using AutoMapper;
using Coin.Api.Grpc;
using Coin.Application.Features.Coins.Commands.CreateCoin;
using Coin.Application.Features.Coins.Commands.DeleteCoin;
using Coin.Application.Features.Coins.Commands.UpdateCoin;
using Coin.Application.Features.Coins.Commands.UpdateCoinStatus;
using Coin.Application.Features.Coins.Queries.ConvertToBoliviano;
using Coin.Application.Features.Coins.Queries.GetCoin;
using Coin.Application.Features.Coins.Queries.GetCoins;
using Grpc.Core;
using MediatR;

namespace Coin.Api.Services;

public class CoinService(ISender sender, IMapper mapper)
    : global::Coin.Api.Grpc.Coin.CoinBase
{
    public override async Task<GetCoinBaseResponsePb> GetCoin(
        GetCoinRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetCoinQuery(request.CoinId),
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
            new CreateCoinCommand(request.Name, request.Code, request.BuyRate),
            context.CancellationToken);

        return mapper.Map<CoinMutationBaseResponsePb>(result);
    }

    public override async Task<CoinMutationBaseResponsePb> UpdateCoin(
        UpdateCoinRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new UpdateCoinCommand(
                request.CoinId, request.Name, request.Code, request.BuyRate, request.IsActive),
            context.CancellationToken);

        return mapper.Map<CoinMutationBaseResponsePb>(result);
    }

    public override async Task<CoinMutationBaseResponsePb> UpdateCoinStatus(
        UpdateCoinStatusRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new UpdateCoinStatusCommand(request.CoinId, request.IsActive),
            context.CancellationToken);

        return mapper.Map<CoinMutationBaseResponsePb>(result);
    }

    public override async Task<CoinMutationBaseResponsePb> DeleteCoin(
        DeleteCoinRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new DeleteCoinCommand(request.CoinId),
            context.CancellationToken);

        return mapper.Map<CoinMutationBaseResponsePb>(result);
    }

    public override async Task<ConvertToBolivianoBaseResponsePb> ConvertToBoliviano(
        ConvertToBolivianoRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new ConvertToBolivianoQuery(request.CoinId, request.Amount),
            context.CancellationToken);

        return mapper.Map<ConvertToBolivianoBaseResponsePb>(result);
    }
}
