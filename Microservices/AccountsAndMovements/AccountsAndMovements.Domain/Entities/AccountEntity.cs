namespace AccountsAndMovements.Domain.Entities;

/// <summary>
/// Cuenta con saldo. El titular puede ser un CLIENTE (extranjero, opera en su
/// moneda) o un COMERCIO (boliviano, siempre en BOB): una sola tabla y un solo
/// mecanismo de saldo para ambos lados del pago.
/// </summary>
public class AccountEntity
{
    public long      AccountId  { get; set; }
    public string    Number     { get; set; } = string.Empty;
    public string    OwnerType  { get; set; } = string.Empty;
    public long      OwnerId    { get; set; }
    public long      CoinId     { get; set; }
    /// <summary>
    /// Codigo de la moneda desnormalizado. Vive aca para que leer una cuenta no
    /// obligue a un salto gRPC a Monedas, y para que el asiento historico
    /// conserve la moneda con la que realmente se opero.
    /// </summary>
    public string    CoinCode   { get; set; } = string.Empty;
    public decimal   Balance    { get; set; }
    public bool      IsActive   { get; set; }
    public DateTime  CreatedAt  { get; set; }
    public DateTime? UpdatedAt  { get; set; }
    public DateTime? DeletedAt  { get; set; }
}
