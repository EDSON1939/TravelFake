namespace AccountsAndMovements.Domain.ExternalServices;

/// <summary>Contrato contra el microservicio de QR.</summary>
public interface IQrService
{
    Task<QrInfo?> GetByCode(string code, CancellationToken ct = default);

    /// <summary>
    /// Marca el QR como utilizado. Se llama despues de que el dinero ya se movio:
    /// el que impide pagar dos veces el mismo QR es el indice unico de este
    /// servicio, no esta llamada, que puede fallar sin dejar plata mal contada.
    /// </summary>
    Task<bool> MarkAsUsed(string code, string transactionCode, CancellationToken ct = default);
}

/// <param name="Amount">Monto que cobra el QR, en <paramref name="CurrencyCode"/> (BOB).</param>
/// <param name="Status">ACTIVE | EXPIRED | USED | CANCELLED.</param>
public record QrInfo(
    string    Code,
    long      MerchantId,
    decimal   Amount,
    string    CurrencyCode,
    string    Reference,
    string    Status,
    DateTime? ExpiresAt);
