using AccountsAndMovements.Application.Common;
using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.ExternalServices;
using AccountsAndMovements.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.CreateMerchantAccount;

/// <summary>
/// Cuenta del comercio boliviano, siempre en BOB. La moneda no se recibe ni se
/// valida: el contrato ya no permite pedir otra.
/// </summary>
public class CreateMerchantAccountCommandHandler(
    IAccountRepository repository,
    IMerchantService merchantService,
    ICurrencyService currencyService)
    : IRequestHandler<CreateMerchantAccountCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(CreateMerchantAccountCommand request, CancellationToken ct)
    {
        var merchant = await merchantService.GetById(request.MerchantId, ct);

        if (merchant is null)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.MERCHANT_NOT_FOUND,
                Domain.Errors.ErrorMessage.MERCHANT_NOT_FOUND);

        if (!merchant.IsActive)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.MERCHANT_INACTIVE,
                Domain.Errors.ErrorMessage.MERCHANT_INACTIVE);

        // BOB tiene que existir en el catalogo igual: de ahi sale el CoinId que
        // se guarda en la cuenta.
        var currency = await currencyService.GetByCode(CurrencyCode.BOB, ct);
        if (currency is null || !currency.IsActive)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.CURRENCY_NOT_SUPPORTED,
                Domain.Errors.ErrorMessage.CURRENCY_NOT_SUPPORTED);

        var id = await repository.Insert(new AccountEntity
        {
            OwnerType = AccountOwnerType.COMERCIO,
            OwnerId   = request.MerchantId,
            CoinId    = currency.CoinId,
            CoinCode  = currency.Code,
            Balance   = request.InitialBalance
        }, ct);

        return AccountCreation.ToResponse(id);
    }
}
