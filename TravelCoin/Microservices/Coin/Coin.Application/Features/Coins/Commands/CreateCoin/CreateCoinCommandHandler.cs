using Coin.Application.Common;
using Coin.Domain.Entities;
using Coin.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Coin.Application.Features.Coins.Commands.CreateCoin;

public class CreateCoinCommandHandler(ICoinRepository repository)
    : IRequestHandler<CreateCoinCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(CreateCoinCommand request, CancellationToken ct)
    {
        var id = await repository.Insert(new CoinEntity
        {
            Name    = request.Name.Trim(),
            Code    = request.Code.Trim().ToUpper(),
            BuyRate = MoneyRules.Parse(request.BuyRate)
        }, ct);

        if (id <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.INSERT_FAILED,
                Domain.Errors.ErrorMessage.INSERT_FAILED);

        return BaseResponse<long>.Success(id);
    }
}
