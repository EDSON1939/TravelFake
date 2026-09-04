using AccountsAndMovements.Application.Common;
using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.ExternalServices;
using AccountsAndMovements.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.CreateCommerceAccount;

/// <summary>
/// Cuenta del comercio boliviano, siempre en BOB. La moneda no se recibe ni se
/// valida: el contrato ya no permite pedir otra.
/// </summary>
public class CreateCommerceAccountCommandHandler(
    IAccountRepository repository,
    ICommerceService commerceService,
    ICurrencyService currencyService)
    : IRequestHandler<CreateCommerceAccountCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(CreateCommerceAccountCommand request, CancellationToken ct)
    {
        var commerce = await commerceService.GetById(request.CommerceId, ct);

        if (commerce is null)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COMMERCE_NOT_FOUND,
                Domain.Errors.ErrorMessage.COMMERCE_NOT_FOUND);

        if (!commerce.IsActive)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COMMERCE_INACTIVE,
                Domain.Errors.ErrorMessage.COMMERCE_INACTIVE);

        // BOB tiene que existir en el catalogo igual: de ahi sale el CoinId que
        // se guarda en la cuenta.
        var currency = await currencyService.GetByCode(CurrencyCode.BOB, ct);
        if (currency is null || !currency.IsActive)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.CURRENCY_NOT_SUPPORTED,
                Domain.Errors.ErrorMessage.CURRENCY_NOT_SUPPORTED);

        var id = await repository.Insert(new AccountEntity
        {
            AccountType = AccountType.COMMERCE,
            HolderId    = request.CommerceId,
            CoinId      = currency.CoinId,
            CoinCode    = currency.Code,
            Balance     = request.InitialBalance
        }, ct);

        return AccountCreation.ToResponse(id);
    }
}
