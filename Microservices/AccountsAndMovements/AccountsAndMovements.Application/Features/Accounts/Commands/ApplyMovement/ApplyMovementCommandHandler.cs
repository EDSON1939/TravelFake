using AccountsAndMovements.Application.Common;
using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Accounts.Commands.ApplyMovement;

/// <summary>
/// Credito o debito suelto sobre una cuenta: recargas del cliente extranjero,
/// ajustes y reversas. El pago QR no pasa por aca, tiene su propia operacion
/// atomica de dos patas.
/// </summary>
public class ApplyMovementCommandHandler(
    IAccountRepository accountRepository,
    IMovementRepository movementRepository)
    : IRequestHandler<ApplyMovementCommand, BaseResponse<MovementAppliedResponse>>
{
    public async Task<BaseResponse<MovementAppliedResponse>> Handle(
        ApplyMovementCommand request, CancellationToken ct)
    {
        var accountNumber = request.AccountNumber.Trim().ToUpper();

        // Toda la mecanica de saldo vive en el SP, en una sola transaccion: aqui
        // solo traducimos su codigo de retorno a un error de negocio.
        var result = await movementRepository.ApplyMovement(new MovementEntity
        {
            AccountNumber  = accountNumber,
            Type           = request.Type,
            Amount         = request.Amount,
            Reference      = request.Reference.Trim(),
            IdempotencyKey = request.IdempotencyKey.Trim(),
            Description    = request.Description.Trim()
        }, ct);

        if (result <= 0)
            return Failure(result);

        // El SP ya dejo el saldo actualizado; lo leemos para devolverlo.
        var account = await accountRepository.GetByNumber(accountNumber, ct);

        return BaseResponse<MovementAppliedResponse>.Success(new MovementAppliedResponse(
            result, accountNumber, account?.Balance ?? 0));
    }

    private static BaseResponse<MovementAppliedResponse> Failure(long result) => result switch
    {
        PaymentResult.ACCOUNT_NOT_FOUND => BaseResponse<MovementAppliedResponse>.Error(
            Domain.Errors.ErrorCode.ACCOUNT_NOT_FOUND,
            Domain.Errors.ErrorMessage.ACCOUNT_NOT_FOUND),

        PaymentResult.ACCOUNT_INACTIVE => BaseResponse<MovementAppliedResponse>.Error(
            Domain.Errors.ErrorCode.ACCOUNT_INACTIVE,
            Domain.Errors.ErrorMessage.ACCOUNT_INACTIVE),

        PaymentResult.INSUFFICIENT_FUNDS => BaseResponse<MovementAppliedResponse>.Error(
            Domain.Errors.ErrorCode.INSUFFICIENT_FUNDS,
            Domain.Errors.ErrorMessage.INSUFFICIENT_FUNDS),

        // La clave ya la uso otra cuenta: no es un reintento de este movimiento.
        PaymentResult.IDEMPOTENCY_CONFLICT => BaseResponse<MovementAppliedResponse>.Error(
            Domain.Errors.ErrorCode.DUPLICATE_TRANSACTION,
            Domain.Errors.ErrorMessage.DUPLICATE_TRANSACTION),

        _ => BaseResponse<MovementAppliedResponse>.Error(
            Domain.Errors.ErrorCode.MOVEMENT_FAILED,
            Domain.Errors.ErrorMessage.MOVEMENT_FAILED)
    };
}
