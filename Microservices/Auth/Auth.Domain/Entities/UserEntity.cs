namespace Auth.Domain.Entities;

public class UserEntity
{
    public long      UserId         { get; set; }
    public string    Username       { get; set; } = string.Empty;

    /// <summary>PBKDF2 en formato "iteraciones.salt.hash". Nunca sale de Auth.</summary>
    public string    PasswordHash   { get; set; } = string.Empty;

    /// <summary>Cliente del negocio asociado. Null en operadores internos.</summary>
    public long?     ClientId       { get; set; }
    public string    FullName       { get; set; } = string.Empty;
    public string    Role           { get; set; } = string.Empty;
    public bool      IsActive       { get; set; }
    public int       FailedAttempts { get; set; }
    public DateTime? LockedUntil    { get; set; }
    public DateTime? LastAccessAt   { get; set; }
    public DateTime  CreatedAt      { get; set; }
    public DateTime? UpdatedAt      { get; set; }
    public DateTime? DeletedAt      { get; set; }

    public bool IsLocked(DateTime now) => LockedUntil.HasValue && LockedUntil.Value > now;
}
