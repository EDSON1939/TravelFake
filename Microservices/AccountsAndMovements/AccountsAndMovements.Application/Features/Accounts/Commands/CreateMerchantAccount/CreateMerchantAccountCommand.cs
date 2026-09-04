using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.CreateMerchantAccount;

/// <summary>
/// Sin moneda: el comercio boliviano cobra en BOB por regla del negocio. Al no
/// ser un parametro, MERCHANT_CURRENCY_INVALID deja de poder ocurrir.
/// </summary>
public record CreateMerchantAccountCommand(
    long    MerchantId,
    decimal InitialBalance) : IRequest<BaseResponse<long>>;
