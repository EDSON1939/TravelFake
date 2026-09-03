using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.ExternalServices;
using AccountsAndMovements.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.CreateAccount;

/// <summary>
/// Crea la cuenta del cliente extranjero o la del comercio. El titular y la
/// moneda se validan contra sus microservicios: este servicio es dueño del
/// saldo, no del catálogo de personas ni del de monedas.
/// </summary>
public class CreateAccountCommandHandler(
    IAccountRepository repository,
    IClientService clientService,
    IMerchantService merchantService,
    ICurrencyService currencyService)
    : IRequestHandler<CreateAccountCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(CreateAccountCommand request, CancellationToken ct)
    {
        var ownerType = request.OwnerType.Trim().ToUpper();
        var coinCode  = request.CoinCode.Trim().ToUpper();

        var ownerError = ownerType == AccountOwnerType.CLIENTE
            ? await ValidateClient(request.OwnerId, ct)
            : await ValidateMerchant(request.OwnerId, coinCode, ct);

        if (ownerError is not null)
            return ownerError;

        var currency = await currencyService.GetByCode(coinCode, ct);
        if (currency is null || !currency.IsActive)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.CURRENCY_NOT_SUPPORTED,
                Domain.Errors.ErrorMessage.CURRENCY_NOT_SUPPORTED);

        var id = await repository.Insert(new AccountEntity
        {
            OwnerType = ownerType,
            OwnerId   = request.OwnerId,
            CoinId    = currency.CoinId,
            CoinCode  = currency.Code,
            Balance   = request.InitialBalance
        }, ct);

        if (id == AccountResult.DUPLICATE)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.ACCOUNT_DUPLICATE,
                Domain.Errors.ErrorMessage.ACCOUNT_DUPLICATE);

        if (id <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.INSERT_FAILED,
                Domain.Errors.ErrorMessage.INSERT_FAILED);

        return BaseResponse<long>.Success(id);
    }

    private async Task<BaseResponse<long>?> ValidateClient(long clientId, CancellationToken ct)
    {
        var client = await clientService.GetById(clientId, ct);

        if (client is null)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.CUSTOMER_NOT_FOUND,
                Domain.Errors.ErrorMessage.CUSTOMER_NOT_FOUND);

        if (!client.IsActive)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.CUSTOMER_INACTIVE,
                Domain.Errors.ErrorMessage.CUSTOMER_INACTIVE);

        return null;
    }

    private async Task<BaseResponse<long>?> ValidateMerchant(long merchantId, string coinCode, CancellationToken ct)
    {
        var merchant = await merchantService.GetById(merchantId, ct);

        if (merchant is null)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.MERCHANT_NOT_FOUND,
                Domain.Errors.ErrorMessage.MERCHANT_NOT_FOUND);

        if (!merchant.IsActive)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.MERCHANT_INACTIVE,
                Domain.Errors.ErrorMessage.MERCHANT_INACTIVE);

        // Regla del reto: el comercio boliviano cobra en BOB. Si se le abriera
        // una cuenta en otra moneda, el pago acreditaria en una moneda que el
        // comercio no puede usar.
        if (coinCode != CurrencyCode.BOB)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.MERCHANT_CURRENCY_INVALID,
                Domain.Errors.ErrorMessage.MERCHANT_CURRENCY_INVALID);

        return null;
    }
}
