using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.CreateCommerceAccount;

/// <summary>
/// Sin moneda: el comercio boliviano cobra en BOB por regla del negocio. Al no
/// ser un parametro, COMMERCE_CURRENCY_INVALID deja de poder ocurrir.
/// </summary>
public record CreateCommerceAccountCommand(
    long    CommerceId,
    decimal InitialBalance) : IRequest<BaseResponse<long>>;
