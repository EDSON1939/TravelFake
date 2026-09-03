using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.CreateAccount;

/// <param name="OwnerType">CLIENTE o COMERCIO.</param>
/// <param name="InitialBalance">
/// Saldo de apertura. No se escribe directo en la cuenta: se asienta como un
/// CREDITO de apertura, asi el libro mayor explica el saldo desde el inicio.
/// </param>
public record CreateAccountCommand(
    string  OwnerType,
    long    OwnerId,
    string  CoinCode,
    decimal InitialBalance) : IRequest<BaseResponse<long>>;
