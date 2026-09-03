using AccountsAndMovements.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.ApplyMovement;

/// <param name="IdempotencyKey">
/// Clave de idempotencia del llamador. Dos envios con la misma clave sobre la
/// misma cuenta aplican el movimiento una sola vez.
/// </param>
public record ApplyMovementCommand(
    string  AccountNumber,
    string  Type,
    decimal Amount,
    string  Reference,
    string  IdempotencyKey,
    string  Description) : IRequest<BaseResponse<MovementAppliedResponse>>;
