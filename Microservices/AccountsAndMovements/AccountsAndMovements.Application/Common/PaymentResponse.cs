namespace AccountsAndMovements.Application.Common;

/// <summary>
/// Resultado del pago QR. Lleva la conversion completa -monto original, tipo de
/// cambio y monto convertido- porque es lo que el reto pide mostrar y lo que
/// quedo grabado en el asiento.
/// </summary>
/// <param name="CommerceName">
/// Solo viene con nombre al ejecutar el pago, que es cuando se valido al
/// comercio. Al consultar una operacion se responde vacio: esa consulta se
/// resuelve con el libro mayor y no depende de que Comercios este arriba.
/// </param>
/// <param name="IsDuplicate">
/// true cuando la clave de idempotencia ya se habia usado y esta respuesta es la
/// de la operacion original, no un cobro nuevo.
/// </param>
public record PaymentResponse(
    string   TransactionCode,
    long     MovementId,
    long     ClientId,
    string   ClientAccountNumber,
    long     CommerceId,
    string   CommerceName,
    string   CommerceAccountNumber,
    string   QrCode,
    decimal  OriginalAmount,
    string   OriginalCurrency,
    decimal  ExchangeRate,
    decimal  ConvertedAmount,
    string   TargetCurrency,
    decimal  ClientBalance,
    string   Status,
    string   Reference,
    string   IdempotencyKey,
    string   Description,
    DateTime CreatedAt,
    bool     IsDuplicate);
