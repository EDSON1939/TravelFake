namespace AccountsAndMovements.Domain.Errors;

public struct ErrorMessage
{
    // ── Exigidos por el reto ─────────────────────────────────────────────────
    public const string CUSTOMER_NOT_FOUND     = "El cliente indicado no existe.";
    public const string CUSTOMER_INACTIVE      = "El cliente indicado se encuentra inactivo.";
    public const string COMMERCE_NOT_FOUND     = "El comercio indicado no existe.";
    public const string COMMERCE_INACTIVE      = "El comercio indicado se encuentra inactivo.";
    public const string QR_NOT_FOUND           = "El código QR no existe.";
    public const string QR_EXPIRED             = "El código QR expiró.";
    public const string QR_ALREADY_USED        = "El código QR ya fue utilizado.";
    public const string QR_INACTIVE            = "El código QR no se encuentra activo.";
    public const string INSUFFICIENT_FUNDS     = "El saldo de la cuenta es insuficiente.";
    public const string CURRENCY_NOT_SUPPORTED = "La moneda indicada no está soportada.";
    public const string INVALID_AMOUNT         = "El monto de la operación no es válido.";
    public const string DUPLICATE_TRANSACTION  = "La clave de idempotencia ya fue usada con datos distintos.";

    // ── Propios del microservicio ────────────────────────────────────────────
    public const string ACCOUNT_NOT_FOUND          = "La cuenta solicitada no existe.";
    public const string ACCOUNT_INACTIVE           = "La cuenta se encuentra inactiva.";
    public const string ACCOUNT_DUPLICATE          = "El titular ya tiene una cuenta en esa moneda.";
    public const string COMMERCE_ACCOUNT_NOT_FOUND = "El comercio no tiene una cuenta habilitada para cobrar.";
    public const string COMMERCE_ACCOUNT_INACTIVE  = "La cuenta del comercio se encuentra inactiva.";
    public const string COMMERCE_CURRENCY_INVALID  = "Las cuentas de comercio solo pueden operar en BOB.";
    public const string CURRENCY_MISMATCH          = "La moneda enviada no corresponde a la cuenta del cliente.";
    public const string QR_TYPE_NOT_SUPPORTED      = "El código QR es de cobro múltiple y este servicio solo liquida QR de un solo uso.";
    public const string EXCHANGE_RATE_NOT_FOUND    = "No existe un tipo de cambio vigente para el par de monedas.";
    public const string INSERT_FAILED              = "No se pudo crear la cuenta. Intente nuevamente.";
    public const string MOVEMENT_FAILED            = "No se pudo aplicar el movimiento. Intente nuevamente.";
    public const string PAYMENT_FAILED             = "No se pudo ejecutar el pago. Intente nuevamente.";
    public const string PAYMENT_NOT_FOUND          = "La operación solicitada no existe.";
}
