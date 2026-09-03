namespace AccountsAndMovements.Domain.Entities;

public struct CurrencyCode
{
    /// <summary>
    /// Moneda de liquidacion del reto: el comercio boliviano siempre cobra en
    /// BOB, sin importar con que moneda pague el cliente extranjero.
    /// </summary>
    public const string BOB = nameof(BOB);
}
