namespace AccountsAndMovements.Domain.Errors;

/// <summary>
/// Los doce primeros codigos son los exigidos por el reto QR Bolivia; el resto
/// cubre los fallos propios de este servicio (cuentas y asientos).
/// </summary>
public struct ErrorCode
{
    // ── Exigidos por el reto ─────────────────────────────────────────────────
    public const string CUSTOMER_NOT_FOUND      = nameof(CUSTOMER_NOT_FOUND);
    public const string CUSTOMER_INACTIVE       = nameof(CUSTOMER_INACTIVE);
    public const string COMMERCE_NOT_FOUND      = nameof(COMMERCE_NOT_FOUND);
    public const string COMMERCE_INACTIVE       = nameof(COMMERCE_INACTIVE);
    public const string QR_NOT_FOUND            = nameof(QR_NOT_FOUND);
    public const string QR_EXPIRED              = nameof(QR_EXPIRED);
    public const string QR_ALREADY_USED         = nameof(QR_ALREADY_USED);
    public const string QR_INACTIVE             = nameof(QR_INACTIVE);
    public const string INSUFFICIENT_FUNDS      = nameof(INSUFFICIENT_FUNDS);
    public const string CURRENCY_NOT_SUPPORTED  = nameof(CURRENCY_NOT_SUPPORTED);
    public const string INVALID_AMOUNT          = nameof(INVALID_AMOUNT);
    public const string DUPLICATE_TRANSACTION   = nameof(DUPLICATE_TRANSACTION);

    // ── Propios del microservicio ────────────────────────────────────────────
    public const string ACCOUNT_NOT_FOUND          = nameof(ACCOUNT_NOT_FOUND);
    public const string ACCOUNT_INACTIVE           = nameof(ACCOUNT_INACTIVE);
    public const string ACCOUNT_DUPLICATE          = nameof(ACCOUNT_DUPLICATE);
    public const string COMMERCE_ACCOUNT_NOT_FOUND = nameof(COMMERCE_ACCOUNT_NOT_FOUND);
    public const string COMMERCE_ACCOUNT_INACTIVE  = nameof(COMMERCE_ACCOUNT_INACTIVE);
    public const string COMMERCE_CURRENCY_INVALID  = nameof(COMMERCE_CURRENCY_INVALID);
    public const string CURRENCY_MISMATCH          = nameof(CURRENCY_MISMATCH);
    public const string QR_TYPE_NOT_SUPPORTED      = nameof(QR_TYPE_NOT_SUPPORTED);
    public const string EXCHANGE_RATE_NOT_FOUND    = nameof(EXCHANGE_RATE_NOT_FOUND);
    public const string INSERT_FAILED              = nameof(INSERT_FAILED);
    public const string MOVEMENT_FAILED            = nameof(MOVEMENT_FAILED);
    public const string PAYMENT_FAILED             = nameof(PAYMENT_FAILED);
    public const string PAYMENT_NOT_FOUND          = nameof(PAYMENT_NOT_FOUND);
}
