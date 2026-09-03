namespace AccountsAndMovements.Domain.Entities;

/// <summary>Estados del QR segun los publica el microservicio de QR.</summary>
public struct QrStatus
{
    public const string ACTIVE    = nameof(ACTIVE);
    public const string EXPIRED   = nameof(EXPIRED);
    public const string USED      = nameof(USED);
    public const string CANCELLED = nameof(CANCELLED);
}
