namespace AccountsAndMovements.Domain.Entities;

public struct AccountOwnerType
{
    /// <summary>Cliente extranjero: opera en la moneda de su pais.</summary>
    public const string CLIENTE  = nameof(CLIENTE);

    /// <summary>Comercio boliviano: por regla del negocio solo cobra en BOB.</summary>
    public const string COMERCIO = nameof(COMERCIO);

    public static readonly string[] All = [CLIENTE, COMERCIO];
}
