namespace AccountsAndMovements.Domain.Services;

/// <summary>
/// Conversion de moneda. Vive en el dominio y no en el handler porque es la
/// regla que decide cuanto cobra el comercio: se prueba sola y no depende de SQL
/// ni de gRPC.
/// </summary>
public static class CurrencyConverter
{
    /// <summary>Decimales con los que se liquida el dinero (BOB usa dos).</summary>
    public const int MoneyScale = 2;

    /// <summary>
    /// 20 USD x 6.96 = 139.20 BOB. Se redondea a dos decimales AwayFromZero: es
    /// el criterio contable habitual, y el resultado es el que se acredita y se
    /// guarda en el asiento, no un valor recalculado despues.
    /// </summary>
    public static decimal Convert(decimal amount, decimal rate)
        => Math.Round(amount * rate, MoneyScale, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Compara dos montos ya redondeados a la escala del dinero. Se usa para
    /// validar que lo convertido cubra exactamente lo que pide el QR.
    /// </summary>
    public static bool AmountsMatch(decimal left, decimal right)
        => Math.Round(left, MoneyScale, MidpointRounding.AwayFromZero)
        == Math.Round(right, MoneyScale, MidpointRounding.AwayFromZero);
}
