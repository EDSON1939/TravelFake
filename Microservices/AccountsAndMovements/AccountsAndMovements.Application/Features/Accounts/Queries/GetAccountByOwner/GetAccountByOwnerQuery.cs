using AccountsAndMovements.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountByOwner;

/// <summary>
/// Resuelve (titular, moneda) -> cuenta. La usan los orquestadores que conocen
/// al cliente o al comercio pero necesitan el numero de cuenta.
/// </summary>
public record GetAccountByOwnerQuery(string OwnerType, long OwnerId, string CoinCode)
    : IRequest<BaseResponse<AccountResponse>>;
