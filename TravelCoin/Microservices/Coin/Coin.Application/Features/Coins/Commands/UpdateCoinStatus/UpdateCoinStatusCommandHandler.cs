using Coin.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Commands.UpdateCoinStatus;

public class UpdateCoinStatusCommandHandler(ICoinRepository repository)
    : IRequestHandler<UpdateCoinStatusCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(UpdateCoinStatusCommand request, CancellationToken ct)
    {
        var affected = await repository.UpdateStatus(request.CoinId, request.IsActive, ct);

        if (affected <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COIN_NOT_FOUND,
                Domain.Errors.ErrorMessage.COIN_NOT_FOUND);

        return BaseResponse<long>.Success(request.CoinId);
    }
}
