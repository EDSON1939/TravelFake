namespace Auth.Domain.Entities;

public struct UserRole
{
    public const string CLIENTE = nameof(CLIENTE);
    public const string AGENTE  = nameof(AGENTE);
    public const string ADMIN   = nameof(ADMIN);

    public static readonly string[] All = [CLIENTE, AGENTE, ADMIN];
}
