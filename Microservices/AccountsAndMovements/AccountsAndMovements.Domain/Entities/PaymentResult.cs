namespace AccountsAndMovements.Domain.Entities;

/// <summary>
/// Codigos que devuelven commerce.EJECUTAR_PAGO_QR y
/// commerce.APLICAR_MOVIMIENTO. Un valor positivo es el ID del movimiento
/// aplicado; los negativos son fallos de negocio, no excepciones: el SP ya hizo
/// rollback de lo que hiciera falta.
/// </summary>
public struct PaymentResult
{
    public const long ACCOUNT_NOT_FOUND          = -1;
    public const long ACCOUNT_INACTIVE           = -2;
    public const long INSUFFICIENT_FUNDS         = -3;
    public const long COMMERCE_ACCOUNT_NOT_FOUND = -4;
    public const long COMMERCE_ACCOUNT_INACTIVE  = -5;
    public const long QR_ALREADY_USED            = -6;
    public const long CURRENCY_MISMATCH          = -7;
    public const long CONVERSION_MISMATCH        = -8;

    /// <summary>
    /// La clave de idempotencia ya existe, pero pertenece a otra cuenta o a otra
    /// operacion. No es un reintento: es una clave reusada.
    /// </summary>
    public const long IDEMPOTENCY_CONFLICT       = -9;
}
