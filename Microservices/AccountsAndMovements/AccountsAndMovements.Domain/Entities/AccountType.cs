namespace AccountsAndMovements.Domain.Entities;

/// <summary>
/// Tipo de cuenta: dice quien es el titular del saldo y, con el, que reglas de
/// moneda aplican. No hay un tercer valor posible en el reto.
/// </summary>
public struct AccountType
{
    /// <summary>Cliente extranjero: opera en la moneda de su pais.</summary>
    public const string CLIENT   = nameof(CLIENT);

    /// <summary>Comercio boliviano: por regla del negocio solo cobra en BOB.</summary>
    public const string COMMERCE = nameof(COMMERCE);

    public static readonly string[] All = [CLIENT, COMMERCE];
}
