using AccountsAndMovements.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Queries.GetAccountByHolder;

/// <summary>
/// Resuelve (titular, moneda) -> cuenta. La usan los orquestadores que conocen
/// al cliente o al comercio pero necesitan el numero de cuenta.
/// </summary>
public record GetAccountByHolderQuery(string AccountType, long HolderId, string CoinCode)
    : IRequest<BaseResponse<AccountResponse>>;
