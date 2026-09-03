using AccountsAndMovements.Application.Common;
using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.ExternalServices;
using AccountsAndMovements.Domain.Repositories;
using AccountsAndMovements.Domain.Services;
using Core.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountsAndMovements.Application.Features.Payments.Commands.ExecuteQrPayment;

/// <summary>
/// Orquesta el pago QR: valida cliente, QR, comercio y moneda contra sus
/// microservicios, convierte el monto y recien entonces le pide al motor que
/// mueva la plata.
///
/// El reparto es deliberado: las validaciones que dependen de datos ajenos se
/// hacen aca, y las dos que solo son confiables con la fila bloqueada -saldo
/// suficiente y QR no cobrado- viven en pay.EJECUTAR_PAGO_QR. Validar el saldo
/// desde aca seria una lectura sin valor: entre la lectura y el pago puede
/// entrar otra operacion.
/// </summary>
public class ExecuteQrPaymentCommandHandler(
    IAccountRepository accountRepository,
    IMovementRepository movementRepository,
    IClientService clientService,
    IMerchantService merchantService,
    IQrService qrService,
    ICurrencyService currencyService,
    ILogger<ExecuteQrPaymentCommandHandler> logger)
    : IRequestHandler<ExecuteQrPaymentCommand, BaseResponse<PaymentResponse>>
{
    public async Task<BaseResponse<PaymentResponse>> Handle(
        ExecuteQrPaymentCommand request, CancellationToken ct)
    {
        var qrCode         = request.QrCode.Trim();
        var currencyCode   = request.CurrencyCode.Trim().ToUpper();
        var idempotencyKey = request.IdempotencyKey.Trim();

        // ── 0. Idempotencia. Antes que nada: si esta operacion ya se ejecuto, la
        //       respuesta correcta es la original, no un cobro nuevo.
        var previous = await movementRepository.GetByIdempotencyKey(idempotencyKey, ct);
        if (previous is not null)
            return Replay(previous, request, qrCode, currencyCode);

        // ── 1-2. Cliente ─────────────────────────────────────────────────────
        var client = await clientService.GetById(request.ClientId, ct);
        if (client is null)
            return Error(Domain.Errors.ErrorCode.CUSTOMER_NOT_FOUND,
                         Domain.Errors.ErrorMessage.CUSTOMER_NOT_FOUND);

        if (!client.IsActive)
            return Error(Domain.Errors.ErrorCode.CUSTOMER_INACTIVE,
                         Domain.Errors.ErrorMessage.CUSTOMER_INACTIVE);

        // ── 3-5. QR: existe, no usado, no expirado, activo ───────────────────
        var qr = await qrService.GetByCode(qrCode, ct);
        if (qr is null)
            return Error(Domain.Errors.ErrorCode.QR_NOT_FOUND,
                         Domain.Errors.ErrorMessage.QR_NOT_FOUND);

        var qrError = ValidateQr(qr);
        if (qrError is not null)
            return qrError;

        // ── 6. Comercio ──────────────────────────────────────────────────────
        var merchant = await merchantService.GetById(qr.MerchantId, ct);
        if (merchant is null)
            return Error(Domain.Errors.ErrorCode.MERCHANT_NOT_FOUND,
                         Domain.Errors.ErrorMessage.MERCHANT_NOT_FOUND);

        if (!merchant.IsActive)
            return Error(Domain.Errors.ErrorCode.MERCHANT_INACTIVE,
                         Domain.Errors.ErrorMessage.MERCHANT_INACTIVE);

        // ── 7. Moneda del cliente ────────────────────────────────────────────
        var currency = await currencyService.GetByCode(currencyCode, ct);
        if (currency is null || !currency.IsActive)
            return Error(Domain.Errors.ErrorCode.CURRENCY_NOT_SUPPORTED,
                         Domain.Errors.ErrorMessage.CURRENCY_NOT_SUPPORTED);

        // El QR cobra en la moneda del comercio. Si no coinciden, el catalogo
        // esta inconsistente y acreditar seria inventar plata en otra moneda.
        var targetCurrency = qr.CurrencyCode;
        if (targetCurrency != merchant.CurrencyCode)
            return Error(Domain.Errors.ErrorCode.CURRENCY_MISMATCH,
                         Domain.Errors.ErrorMessage.CURRENCY_MISMATCH);

        // ── 8. Tipo de cambio ────────────────────────────────────────────────
        decimal rate;
        if (currencyCode == targetCurrency)
        {
            // Un boliviano pagando en BOB no necesita conversion.
            rate = 1m;
        }
        else
        {
            var quoted = await currencyService.GetExchangeRate(currencyCode, targetCurrency, ct);
            if (quoted is null)
                return Error(Domain.Errors.ErrorCode.EXCHANGE_RATE_NOT_FOUND,
                             Domain.Errors.ErrorMessage.EXCHANGE_RATE_NOT_FOUND);

            rate = quoted.Value;
        }

        // ── 9. Conversion ────────────────────────────────────────────────────
        var convertedAmount = CurrencyConverter.Convert(request.Amount, rate);

        // Un QR con monto fijo tiene que cobrarse por ese monto exacto: ni de
        // mas, ni de menos. Monto 0 significa QR abierto y acepta cualquiera.
        if (qr.Amount > 0 && !CurrencyConverter.AmountsMatch(convertedAmount, qr.Amount))
            return Error(Domain.Errors.ErrorCode.INVALID_AMOUNT,
                         Domain.Errors.ErrorMessage.INVALID_AMOUNT);

        // ── 10. Cuentas. Esto NO consulta saldo para decidir: solo traduce
        //        (titular, moneda) al numero que necesita el SP.
        var clientAccount = await accountRepository.GetByOwnerAndCoin(
            AccountOwnerType.CLIENTE, client.ClientId, currencyCode, ct);
        if (clientAccount is null)
            return Error(Domain.Errors.ErrorCode.ACCOUNT_NOT_FOUND,
                         Domain.Errors.ErrorMessage.ACCOUNT_NOT_FOUND);

        var merchantAccount = await accountRepository.GetByOwnerAndCoin(
            AccountOwnerType.COMERCIO, merchant.MerchantId, targetCurrency, ct);
        if (merchantAccount is null)
            return Error(Domain.Errors.ErrorCode.MERCHANT_ACCOUNT_NOT_FOUND,
                         Domain.Errors.ErrorMessage.MERCHANT_ACCOUNT_NOT_FOUND);

        // ── 11-13. Ejecucion atomica: debito, credito y los dos asientos ─────
        var transactionCode = Guid.NewGuid().ToString();

        var result = await movementRepository.ExecuteQrPayment(new PaymentEntity
        {
            ClientAccountNumber   = clientAccount.Number,
            MerchantAccountNumber = merchantAccount.Number,
            MerchantId            = merchant.MerchantId,
            QrCode                = qrCode,
            OriginalAmount        = request.Amount,
            OriginalCurrency      = currencyCode,
            ExchangeRate          = rate,
            ConvertedAmount       = convertedAmount,
            TargetCurrency        = targetCurrency,
            Reference             = qr.Reference,
            IdempotencyKey        = idempotencyKey,
            TransactionCode       = transactionCode,
            Description           = request.Description.Trim()
        }, ct);

        if (result <= 0)
            return Failure(result);

        // El asiento guardado es la fuente de verdad: si otro hilo gano la
        // carrera con la misma clave, el SP devolvio SU movimiento y es ese el
        // que hay que responder, no los valores que trae este request.
        var applied = await movementRepository.GetByIdempotencyKey(idempotencyKey, ct);

        // ── 14. Marcar el QR como usado ──────────────────────────────────────
        // Best effort a proposito: el dinero ya se movio y esta llamada no puede
        // deshacerlo. Que el QR no se pague dos veces lo garantiza el indice
        // unico UQ_PAY_MOVIMIENTO_QR, no el estado que guarde el servicio de QR.
        var marked = await qrService.MarkAsUsed(qrCode, applied?.TransactionCode ?? transactionCode, ct);
        if (!marked)
            logger.LogWarning(
                "Pago {Transaction} aplicado pero el QR {QrCode} no pudo marcarse como usado en el servicio de QR.",
                applied?.TransactionCode ?? transactionCode, qrCode);

        // ── 15. Resultado ────────────────────────────────────────────────────
        return BaseResponse<PaymentResponse>.Success(new PaymentResponse(
            applied?.TransactionCode ?? transactionCode,
            result,
            client.ClientId,
            clientAccount.Number,
            merchant.MerchantId,
            merchant.Name,
            merchantAccount.Number,
            qrCode,
            request.Amount,
            currencyCode,
            rate,
            convertedAmount,
            targetCurrency,
            applied?.BalanceAfter ?? clientAccount.Balance - request.Amount,
            MovementStatus.COMPLETED,
            qr.Reference,
            idempotencyKey,
            request.Description.Trim(),
            applied?.CreatedAt ?? DateTime.Now,
            IsDuplicate: false));
    }

    /// <summary>
    /// Reintento de una operacion ya ejecutada. Se responde el resultado
    /// original -incluido el saldo que quedo entonces- sin volver a cobrar.
    ///
    /// Si la clave viene con datos distintos NO es un reintento sino una clave
    /// reusada, y devolver el resultado anterior le ocultaria al llamador que su
    /// segundo pago nunca ocurrio: por eso es DUPLICATE_TRANSACTION.
    /// </summary>
    private static BaseResponse<PaymentResponse> Replay(
        MovementEntity previous, ExecuteQrPaymentCommand request, string qrCode, string currencyCode)
    {
        var sameOperation = previous.OwnerId          == request.ClientId
                         && previous.QrCode           == qrCode
                         && previous.OriginalCurrency == currencyCode
                         && previous.OriginalAmount   == request.Amount;

        if (!sameOperation)
            return Error(Domain.Errors.ErrorCode.DUPLICATE_TRANSACTION,
                         Domain.Errors.ErrorMessage.DUPLICATE_TRANSACTION);

        return BaseResponse<PaymentResponse>.Success(new PaymentResponse(
            previous.TransactionCode,
            previous.MovementId,
            previous.OwnerId,
            previous.AccountNumber,
            previous.MerchantId ?? 0,
            string.Empty,
            string.Empty,
            previous.QrCode ?? qrCode,
            previous.OriginalAmount,
            previous.OriginalCurrency,
            previous.ExchangeRate,
            previous.ConvertedAmount,
            previous.TargetCurrency,
            previous.BalanceAfter,
            previous.Status,
            previous.Reference,
            previous.IdempotencyKey,
            previous.Description,
            previous.CreatedAt,
            IsDuplicate: true));
    }

    private static BaseResponse<PaymentResponse>? ValidateQr(QrInfo qr)
    {
        if (qr.Status == QrStatus.USED)
            return Error(Domain.Errors.ErrorCode.QR_ALREADY_USED,
                         Domain.Errors.ErrorMessage.QR_ALREADY_USED);

        // Expirado por estado o por fecha: el servicio de QR puede no haber
        // pasado todavia el barrido que cambia el estado, y la fecha manda.
        if (qr.Status == QrStatus.EXPIRED || (qr.ExpiresAt.HasValue && qr.ExpiresAt.Value <= DateTime.Now))
            return Error(Domain.Errors.ErrorCode.QR_EXPIRED,
                         Domain.Errors.ErrorMessage.QR_EXPIRED);

        if (qr.Status != QrStatus.ACTIVE)
            return Error(Domain.Errors.ErrorCode.QR_INACTIVE,
                         Domain.Errors.ErrorMessage.QR_INACTIVE);

        return null;
    }

    private static BaseResponse<PaymentResponse> Failure(long result) => result switch
    {
        PaymentResult.ACCOUNT_NOT_FOUND => Error(
            Domain.Errors.ErrorCode.ACCOUNT_NOT_FOUND,
            Domain.Errors.ErrorMessage.ACCOUNT_NOT_FOUND),

        PaymentResult.ACCOUNT_INACTIVE => Error(
            Domain.Errors.ErrorCode.ACCOUNT_INACTIVE,
            Domain.Errors.ErrorMessage.ACCOUNT_INACTIVE),

        PaymentResult.INSUFFICIENT_FUNDS => Error(
            Domain.Errors.ErrorCode.INSUFFICIENT_FUNDS,
            Domain.Errors.ErrorMessage.INSUFFICIENT_FUNDS),

        PaymentResult.MERCHANT_ACCOUNT_NOT_FOUND => Error(
            Domain.Errors.ErrorCode.MERCHANT_ACCOUNT_NOT_FOUND,
            Domain.Errors.ErrorMessage.MERCHANT_ACCOUNT_NOT_FOUND),

        PaymentResult.MERCHANT_ACCOUNT_INACTIVE => Error(
            Domain.Errors.ErrorCode.MERCHANT_ACCOUNT_INACTIVE,
            Domain.Errors.ErrorMessage.MERCHANT_ACCOUNT_INACTIVE),

        PaymentResult.QR_ALREADY_USED => Error(
            Domain.Errors.ErrorCode.QR_ALREADY_USED,
            Domain.Errors.ErrorMessage.QR_ALREADY_USED),

        PaymentResult.CURRENCY_MISMATCH => Error(
            Domain.Errors.ErrorCode.CURRENCY_MISMATCH,
            Domain.Errors.ErrorMessage.CURRENCY_MISMATCH),

        PaymentResult.CONVERSION_MISMATCH => Error(
            Domain.Errors.ErrorCode.INVALID_AMOUNT,
            Domain.Errors.ErrorMessage.INVALID_AMOUNT),

        // La clave existe pero es de otra cuenta u otra operacion. Solo llega
        // aca por una carrera: el pre-check del handler ya atrapa el caso normal.
        PaymentResult.IDEMPOTENCY_CONFLICT => Error(
            Domain.Errors.ErrorCode.DUPLICATE_TRANSACTION,
            Domain.Errors.ErrorMessage.DUPLICATE_TRANSACTION),

        _ => Error(Domain.Errors.ErrorCode.PAYMENT_FAILED,
                   Domain.Errors.ErrorMessage.PAYMENT_FAILED)
    };

    private static BaseResponse<PaymentResponse> Error(string code, string message)
        => BaseResponse<PaymentResponse>.Error(code, message);
}
