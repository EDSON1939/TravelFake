namespace Coin.Domain.Entities;

/// <summary>
/// Monetary constants of the domain. Every conversion targets the boliviano,
/// which is the agency's base currency.
/// </summary>
public static class Money
{
    /// <summary>Precision of MONEDAS_TCCOMPRA_DC — DECIMAL(18,2).</summary>
    public const int Decimals = 2;

    /// <summary>ISO 4217 code of the boliviano.</summary>
    public const string BolivianoCode = "BOB";

    /// <summary>Target currency name, used in conversion responses.</summary>
    public const string BolivianoName = "Boliviano";
}
