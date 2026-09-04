namespace Coin.Domain.Entities;

/// <summary>
/// Projection of the TRAVELFAKE..MONEDAS table.
/// Property names match the aliases returned by
/// travelfake.GET_MONEDA_ALL and travelfake.GET_MONEDA_BY_ID.
/// </summary>
public class CoinEntity
{
    public long      CoinId    { get; set; }
    public string    Name      { get; set; } = string.Empty;
    public string    Code      { get; set; } = string.Empty;

    /// <summary>MONEDAS_TCCOMPRA_DC: how many bolivianos one unit of this coin is worth.</summary>
    public decimal   BuyRate   { get; set; }

    public bool      IsActive  { get; set; }
    public DateTime  CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Converts an amount expressed in this coin to bolivianos using the buy rate.
    /// The result is rounded to 2 decimals (away from zero), which is the
    /// precision of MONEDAS_TCCOMPRA_DC. To convert bolivianos to bolivianos just
    /// register the BOB coin with MONEDAS_TCCOMPRA_DC = 1 and it stays one to one.
    /// </summary>
    public decimal ToBoliviano(decimal amount)
        => decimal.Round(amount * BuyRate, Money.Decimals, MidpointRounding.AwayFromZero);
}
