namespace AccountsAndMovements.Domain.ExternalServices;

/// <summary>Contrato contra el microservicio de Comercios.</summary>
public interface IMerchantService
{
    Task<MerchantInfo?> GetById(long merchantId, CancellationToken ct = default);
}

/// <param name="CurrencyCode">Por regla del reto, siempre BOB.</param>
public record MerchantInfo(
    long   MerchantId,
    string Name,
    string Nit,
    string CurrencyCode,
    bool   IsActive);
