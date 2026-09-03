namespace AccountsAndMovements.Domain.Entities;

public struct MovementType
{
    public const string CREDITO = nameof(CREDITO);
    public const string DEBITO  = nameof(DEBITO);

    public static readonly string[] All = [CREDITO, DEBITO];
}
