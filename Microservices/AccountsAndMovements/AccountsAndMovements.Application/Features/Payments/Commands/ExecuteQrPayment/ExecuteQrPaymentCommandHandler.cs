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
/// suficiente y QR no cobrado- viven en commerce.EJECUTAR_PAGO_QR. Validar el
/// saldo desde aca seria una lectura sin valor: entre la lectura y el pago
/// puede entrar otra operacion.
/// </summary>
public class ExecuteQrPaymentCommandHandler(
    IAccountRepository accountRepository,
    IMovementRepository movementRepository,
    IClientService clientService,
    ICommerceService commerceService,
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
        var commerce = await commerceService.GetById(qr.CommerceId, ct);
        if (commerce is null)
            return Error(Domain.Errors.ErrorCode.COMMERCE_NOT_FOUND,
                         Domain.Errors.ErrorMessage.COMMERCE_NOT_FOUND);

        if (!commerce.IsActive)
            return Error(Domain.Errors.ErrorCode.COMMERCE_INACTIVE,
                         Domain.Errors.ErrorMessage.COMMERCE_INACTIVE);

        // ── 7. Moneda del cliente ────────────────────────────────────────────
        var currency = await currencyService.GetByCode(currencyCode, ct);
        if (currency is null || !currency.IsActive)
            return Error(Domain.Errors.ErrorCode.CURRENCY_NOT_SUPPORTED,
                         Domain.Errors.ErrorMessage.CURRENCY_NOT_SUPPORTED);

        // El contrato oficial de QR no publica moneda de cobro: la unica fuente
        // es el comercio, que por regla del reto siempre liquida en BOB. No se
        // pone aca una comparacion entre dos constantes que valen lo mismo: la
        // verificacion que protege la plata es la del SP, que compara contra la
        // moneda REAL de las dos cuentas y devuelve CURRENCY_MISMATCH.
        var targetCurrency = commerce.CurrencyCode;

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
        var clientAccount = await accountRepository.GetByHolderAndCoin(
            AccountType.CLIENT, client.ClientId, currencyCode, ct);
        if (clientAccount is null)
            return Error(Domain.Errors.ErrorCode.ACCOUNT_NOT_FOUND,
                         Domain.Errors.ErrorMessage.ACCOUNT_NOT_FOUND);

        var commerceAccount = await accountRepository.GetByHolderAndCoin(
            AccountType.COMMERCE, commerce.CommerceId, targetCurrency, ct);
        if (commerceAccount is null)
            return Error(Domain.Errors.ErrorCode.COMMERCE_ACCOUNT_NOT_FOUND,
                         Domain.Errors.ErrorMessage.COMMERCE_ACCOUNT_NOT_FOUND);

        // ── 11-13. Ejecucion atomica: debito, credito y los dos asientos ─────
        var transactionCode = Guid.NewGuid().ToString();

        // El contrato oficial no publica referencia. Se guarda el id del QR en su
        // microservicio: es lo que permite volver a esa fila desde el asiento,
        // cosa que el codigo -ya guardado en MOVI_QR_CODIGO_VC- no aportaria de
        // nuevo.
        var reference = $"QR-{qr.QrId}";

        var result = await movementRepository.ExecuteQrPayment(new PaymentEntity
        {
            ClientAccountNumber   = clientAccount.Number,
            CommerceAccountNumber = commerceAccount.Number,
            CommerceId            = commerce.CommerceId,
            QrCode                = qrCode,
            OriginalAmount        = request.Amount,
            OriginalCurrency      = currencyCode,
            ExchangeRate          = rate,
            ConvertedAmount       = convertedAmount,
            TargetCurrency        = targetCurrency,
            Reference             = reference,
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

        // ── 14. Consumir el QR ───────────────────────────────────────────────
        // Best effort a proposito: el dinero ya se movio y esta llamada no puede
        // deshacerlo. Que el QR no se pague dos veces lo garantiza el indice
        // unico UQ_COMMERCE_MOVIMIENTO_QR, no el estado que guarde el servicio
        // de QR.
        var consumed = await qrService.Consume(qrCode, ct);
        if (!consumed)
            logger.LogWarning(
                "Pago {Transaction} aplicado pero el QR {QrCode} no pudo consumirse en el servicio de QR.",
                applied?.TransactionCode ?? transactionCode, qrCode);

        // ── 15. Resultado ────────────────────────────────────────────────────
        return BaseResponse<PaymentResponse>.Success(new PaymentResponse(
            applied?.TransactionCode ?? transactionCode,
            result,
            client.ClientId,
            clientAccount.Number,
            commerce.CommerceId,
            commerce.Name,
            commerceAccount.Number,
            qrCode,
            request.Amount,
            currencyCode,
            rate,
            convertedAmount,
            targetCurrency,
            applied?.BalanceAfter ?? clientAccount.Balance - request.Amount,
            MovementStatus.COMPLETED,
            applied?.Reference ?? reference,
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
        var sameOperation = previous.HolderId         == request.ClientId
                         && previous.QrCode           == qrCode
                         && previous.OriginalCurrency == currencyCode
                         && previous.OriginalAmount   == request.Amount;

        if (!sameOperation)
            return Error(Domain.Errors.ErrorCode.DUPLICATE_TRANSACTION,
                         Domain.Errors.ErrorMessage.DUPLICATE_TRANSACTION);

        return BaseResponse<PaymentResponse>.Success(new PaymentResponse(
            previous.TransactionCode,
            previous.MovementId,
            previous.HolderId,
            previous.AccountNumber,
            previous.CommerceId ?? 0,
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

    /// <summary>
    /// El contrato publica es_valido, pero es solo "estado == ACTIVE": usarlo
    /// colapsaria en un unico error los tres casos que el reto pide distinguir.
    /// Por eso se miran estado, activo y fecha por separado.
    ///
    /// El orden importa: primero lo que ya ocurrio (usado), despues lo que
    /// caduco, y al final lo que nunca estuvo habilitado.
    /// </summary>
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

        // Dos formas de estar deshabilitado: estado distinto de ACTIVE, o el
        // flag activo en false. El contrato oficial da de baja un QR por el
        // flag, dejando el estado en ACTIVE, asi que mirar solo el estado
        // dejaria pasar a cobrar un QR retirado.
        if (qr.Status != QrStatus.ACTIVE || !qr.IsActive)
            return Error(Domain.Errors.ErrorCode.QR_INACTIVE,
                         Domain.Errors.ErrorMessage.QR_INACTIVE);

        // Un QR MULTIPLE se cobra muchas veces y el libro mayor no lo admite:
        // UQ_COMMERCE_MOVIMIENTO_QR deja un unico DEBITO por codigo. Sin este
        // corte el primer cobro saldria bien y el segundo devolveria
        // QR_ALREADY_USED, que para un QR recurrente es una mentira.
        if (qr.Type == QrType.MULTIPLE)
            return Error(Domain.Errors.ErrorCode.QR_TYPE_NOT_SUPPORTED,
                         Domain.Errors.ErrorMessage.QR_TYPE_NOT_SUPPORTED);

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

        PaymentResult.COMMERCE_ACCOUNT_NOT_FOUND => Error(
            Domain.Errors.ErrorCode.COMMERCE_ACCOUNT_NOT_FOUND,
            Domain.Errors.ErrorMessage.COMMERCE_ACCOUNT_NOT_FOUND),

        PaymentResult.COMMERCE_ACCOUNT_INACTIVE => Error(
            Domain.Errors.ErrorCode.COMMERCE_ACCOUNT_INACTIVE,
            Domain.Errors.ErrorMessage.COMMERCE_ACCOUNT_INACTIVE),

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
