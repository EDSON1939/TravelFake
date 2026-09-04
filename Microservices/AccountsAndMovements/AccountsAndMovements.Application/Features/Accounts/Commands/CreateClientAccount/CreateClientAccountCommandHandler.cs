using AccountsAndMovements.Application.Common;
using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.ExternalServices;
using AccountsAndMovements.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.CreateClientAccount;

/// <summary>
/// Cuenta del cliente extranjero, en la moneda de su pais. El titular y la
/// moneda se validan contra sus microservicios: este servicio es dueño del
/// saldo, no del catálogo de personas ni del de monedas.
/// </summary>
public class CreateClientAccountCommandHandler(
    IAccountRepository repository,
    IClientService clientService,
    ICurrencyService currencyService)
    : IRequestHandler<CreateClientAccountCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(CreateClientAccountCommand request, CancellationToken ct)
    {
        var coinCode = request.CoinCode.Trim().ToUpper();

        var client = await clientService.GetById(request.ClientId, ct);

        if (client is null)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.CUSTOMER_NOT_FOUND,
                Domain.Errors.ErrorMessage.CUSTOMER_NOT_FOUND);

        if (!client.IsActive)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.CUSTOMER_INACTIVE,
                Domain.Errors.ErrorMessage.CUSTOMER_INACTIVE);

        var currency = await currencyService.GetByCode(coinCode, ct);
        if (currency is null || !currency.IsActive)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.CURRENCY_NOT_SUPPORTED,
                Domain.Errors.ErrorMessage.CURRENCY_NOT_SUPPORTED);

        var id = await repository.Insert(new AccountEntity
        {
            AccountType = AccountType.CLIENT,
            HolderId    = request.ClientId,
            CoinId      = currency.CoinId,
            CoinCode    = currency.Code,
            Balance     = request.InitialBalance
        }, ct);

        return AccountCreation.ToResponse(id);
    }
}
