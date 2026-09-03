namespace AccountsAndMovements.Domain.Entities;

/// <summary>
/// Datos ya validados y convertidos que se le entregan a pay.EJECUTAR_PAGO_QR.
/// Cuando llega aca no queda ninguna decision de negocio pendiente salvo el
/// saldo, que solo puede resolverse con la fila de la cuenta bloqueada.
/// </summary>
public class PaymentEntity
{
    public string  ClientAccountNumber   { get; set; } = string.Empty;
    public string  MerchantAccountNumber { get; set; } = string.Empty;
    public long    MerchantId            { get; set; }
    public string  QrCode                { get; set; } = string.Empty;

    /// <summary>Monto y moneda con los que paga el cliente (ej: 20 USD).</summary>
    public decimal OriginalAmount        { get; set; }
    public string  OriginalCurrency      { get; set; } = string.Empty;

    /// <summary>Tipo de cambio aplicado. Queda grabado en el asiento.</summary>
    public decimal ExchangeRate          { get; set; }

    /// <summary>Monto y moneda que recibe el comercio (ej: 139.20 BOB).</summary>
    public decimal ConvertedAmount       { get; set; }
    public string  TargetCurrency        { get; set; } = string.Empty;

    public string  Reference             { get; set; } = string.Empty;
    public string  IdempotencyKey        { get; set; } = string.Empty;
    public string  TransactionCode       { get; set; } = string.Empty;
    public string  Description           { get; set; } = string.Empty;
}
