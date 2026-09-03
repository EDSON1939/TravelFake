using AccountsAndMovements.Application.Common;
using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace AccountsAndMovements.Application.Features.Payments.Queries.GetPayment;

/// <summary>
/// Se resuelve solo con el libro mayor: no llama a Comercios ni a Monedas. Una
/// consulta de algo ya ocurrido no deberia caerse porque otro servicio este
/// abajo, y los datos de la conversion ya quedaron grabados en el asiento.
/// </summary>
public class GetPaymentQueryHandler(IMovementRepository repository)
    : IRequestHandler<GetPaymentQuery, BaseResponse<PaymentResponse>>
{
    public async Task<BaseResponse<PaymentResponse>> Handle(GetPaymentQuery request, CancellationToken ct)
    {
        var transactionCode = request.TransactionCode.Trim();

        if (string.IsNullOrEmpty(transactionCode))
        {
            var byKey = await repository.GetByIdempotencyKey(request.IdempotencyKey.Trim(), ct);
            if (byKey is null)
                return NotFound();

            transactionCode = byKey.TransactionCode;
        }

        var movements = (await repository.GetByTransactionCode(transactionCode, ct)).ToList();

        var debit = movements.FirstOrDefault(m => m.Type == MovementType.DEBITO);
        if (debit is null)
            return NotFound();

        var credit = movements.FirstOrDefault(m => m.Type == MovementType.CREDITO);

        return BaseResponse<PaymentResponse>.Success(new PaymentResponse(
            debit.TransactionCode,
            debit.MovementId,
            debit.OwnerId,
            debit.AccountNumber,
            debit.MerchantId ?? 0,
            // El nombre del comercio no vive en el asiento: la fuente es el
            // microservicio de Comercios, y esta consulta no depende de el.
            string.Empty,
            credit?.AccountNumber ?? string.Empty,
            debit.QrCode ?? string.Empty,
            debit.OriginalAmount,
            debit.OriginalCurrency,
            debit.ExchangeRate,
            debit.ConvertedAmount,
            debit.TargetCurrency,
            debit.BalanceAfter,
            debit.Status,
            debit.Reference,
            debit.IdempotencyKey,
            debit.Description,
            debit.CreatedAt,
            IsDuplicate: false));
    }

    private static BaseResponse<PaymentResponse> NotFound()
        => BaseResponse<PaymentResponse>.Error(
            Domain.Errors.ErrorCode.PAYMENT_NOT_FOUND,
            Domain.Errors.ErrorMessage.PAYMENT_NOT_FOUND);
}
