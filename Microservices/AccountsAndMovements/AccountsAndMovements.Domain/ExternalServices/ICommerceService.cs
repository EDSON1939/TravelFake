namespace AccountsAndMovements.Domain.ExternalServices;

/// <summary>Contrato contra el microservicio de Comercios.</summary>
public interface ICommerceService
{
    Task<CommerceInfo?> GetById(long commerceId, CancellationToken ct = default);
}

/// <param name="CurrencyCode">
///   Siempre BOB. El contrato de Comercios no lo publica; es la regla del reto.
/// </param>
public record CommerceInfo(
    long   CommerceId,
    string Name,
    string Nit,
    string CurrencyCode,
    bool   IsActive);
