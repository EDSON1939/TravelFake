using AccountsAndMovements.Domain.ExternalServices;

namespace AccountsAndMovements.Infrastructure.ExternalServices.Fakes;

/// <summary>
/// Catalogo de monedas y tipos de cambio quemados. El CoinId es el que termina
/// guardado en CUEN_MONEDA_ID_IT, asi que se mantiene estable entre corridas.
/// </summary>
public class FakeCurrencyService : ICurrencyService
{
    private static readonly Dictionary<string, CurrencyInfo> Currencies =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["BOB"] = new CurrencyInfo(1, "BOB", "Bs", IsActive: true),
            ["USD"] = new CurrencyInfo(2, "USD", "$",  IsActive: true),
            ["PEN"] = new CurrencyInfo(3, "PEN", "S/", IsActive: true),
            ["EUR"] = new CurrencyInfo(4, "EUR", "E",  IsActive: true),
            // Para probar CURRENCY_NOT_SUPPORTED con una moneda que existe pero
            // esta dada de baja en el catalogo.
            ["ARS"] = new CurrencyInfo(5, "ARS", "$",  IsActive: false),
        };

    private static readonly Dictionary<string, decimal> Rates =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["USD>BOB"] = 6.96m,
            ["PEN>BOB"] = 1.85m,
            ["EUR>BOB"] = 7.50m,
        };

    public Task<CurrencyInfo?> GetByCode(string code, CancellationToken ct = default)
        => Task.FromResult<CurrencyInfo?>(Currencies.TryGetValue(code, out var currency) ? currency : null);

    public Task<decimal?> GetExchangeRate(string from, string to, CancellationToken ct = default)
    {
        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<decimal?>(1m);

        return Task.FromResult<decimal?>(Rates.TryGetValue($"{from}>{to}", out var rate) ? rate : null);
    }
}
