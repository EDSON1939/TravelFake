using Coin.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Commands.DeleteCoin;

public class DeleteCoinCommandHandler(ICoinRepository repository)
    : IRequestHandler<DeleteCoinCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(DeleteCoinCommand request, CancellationToken ct)
    {
        var affected = await repository.Delete(request.CoinId, ct);

        // travelfake.DELETE_MONEDA returns 0 when MONEDAS_ID_IT does not exist
        if (affected <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COIN_NOT_FOUND,
                Domain.Errors.ErrorMessage.COIN_NOT_FOUND);

        return BaseResponse<long>.Success(request.CoinId);
    }
}
