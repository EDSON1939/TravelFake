namespace AccountsAndMovements.Domain.ExternalServices;

/// <summary>Contrato contra el microservicio de Clientes.</summary>
public interface IClientService
{
    Task<ClientInfo?> GetById(long clientId, CancellationToken ct = default);
}

/// <param name="CurrencyCode">Moneda con la que el cliente tiene fondos (ej: PEN).</param>
public record ClientInfo(
    long   ClientId,
    string FullName,
    string Email,
    string CountryCode,
    string CurrencyCode,
    bool   IsActive);
