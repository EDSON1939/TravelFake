namespace AccountsAndMovements.Domain.Entities;

/// <summary>Estados exigidos por el reto para el historial de transacciones.</summary>
public struct MovementStatus
{
    public const string PENDING   = nameof(PENDING);
    public const string COMPLETED = nameof(COMPLETED);
    public const string FAILED    = nameof(FAILED);
    public const string CANCELLED = nameof(CANCELLED);

    public static readonly string[] All = [PENDING, COMPLETED, FAILED, CANCELLED];
}
