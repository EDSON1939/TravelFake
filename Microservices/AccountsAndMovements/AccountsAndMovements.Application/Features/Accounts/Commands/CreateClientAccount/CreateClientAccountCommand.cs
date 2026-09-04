using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.CreateClientAccount;

/// <param name="CoinCode">Moneda del cliente extranjero: PEN, USD, EUR...</param>
/// <param name="InitialBalance">
/// Saldo de apertura. No se escribe directo en la cuenta: se asienta como un
/// CREDITO de apertura, asi el libro mayor explica el saldo desde el inicio.
/// </param>
public record CreateClientAccountCommand(
    long    ClientId,
    string  CoinCode,
    decimal InitialBalance) : IRequest<BaseResponse<long>>;
