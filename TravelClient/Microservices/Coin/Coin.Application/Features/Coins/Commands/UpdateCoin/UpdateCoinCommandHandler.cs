using Coin.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Commands.UpdateCoin;

public class UpdateCoinCommandHandler(ICoinRepository repository)
    : IRequestHandler<UpdateCoinCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(UpdateCoinCommand request, CancellationToken ct)
    {
        var affected = await repository.UpdateStatus(request.CoinId, request.IsActive, ct);

        if (affected <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.UPDATE_FAILED,
                Domain.Errors.ErrorMessage.UPDATE_FAILED);

        return BaseResponse<long>.Success(request.CoinId);
    }
}
