namespace AccountsAndMovements.Domain.Entities;

/// <summary>
/// Asiento del libro mayor. Un pago QR produce dos: el DEBITO de la cuenta del
/// cliente y el CREDITO de la cuenta del comercio, ambos con el mismo
/// TransactionCode. Los campos de conversion se guardan en cada asiento porque
/// la operacion historica no debe cambiar si manana cambia el tipo de cambio.
/// </summary>
public class MovementEntity
{
    public long     MovementId       { get; set; }
    public long     AccountId        { get; set; }
    public string   AccountNumber    { get; set; } = string.Empty;
    public string   OwnerType        { get; set; } = string.Empty;
    public long     OwnerId          { get; set; }
    public string   Type             { get; set; } = string.Empty;
    public string   Status           { get; set; } = string.Empty;

    /// <summary>Monto movido en la moneda de la cuenta.</summary>
    public decimal  Amount           { get; set; }
    public decimal  BalanceBefore    { get; set; }
    public decimal  BalanceAfter     { get; set; }

    // ── Trazabilidad de la conversion, congelada al momento del pago ──────────
    public decimal  OriginalAmount   { get; set; }
    public string   OriginalCurrency { get; set; } = string.Empty;
    public decimal  ExchangeRate     { get; set; }
    public decimal  ConvertedAmount  { get; set; }
    public string   TargetCurrency   { get; set; } = string.Empty;

    public long?    MerchantId       { get; set; }
    public string?  QrCode           { get; set; }
    public string   Reference        { get; set; } = string.Empty;
    /// <summary>Clave de idempotencia enviada por el llamador (ej: TX-2026-000001).</summary>
    public string   IdempotencyKey   { get; set; } = string.Empty;
    /// <summary>Codigo de la operacion. Comparten valor el debito y el credito del mismo pago.</summary>
    public string   TransactionCode  { get; set; } = string.Empty;
    public string   Description      { get; set; } = string.Empty;
    public DateTime CreatedAt        { get; set; }
}
