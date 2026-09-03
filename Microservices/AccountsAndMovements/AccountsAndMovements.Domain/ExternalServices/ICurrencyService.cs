namespace AccountsAndMovements.Domain.ExternalServices;

/// <summary>Contrato contra el microservicio de Monedas.</summary>
public interface ICurrencyService
{
    Task<CurrencyInfo?> GetByCode(string code, CancellationToken ct = default);

    /// <summary>
    /// Tipo de cambio vigente de <paramref name="from"/> a <paramref name="to"/>.
    /// El valor devuelto se congela en el asiento: la operacion historica no
    /// cambia si el tipo de cambio se mueve despues.
    /// </summary>
    Task<decimal?> GetExchangeRate(string from, string to, CancellationToken ct = default);
}

public record CurrencyInfo(
    long   CoinId,
    string Code,
    string Symbol,
    bool   IsActive);
