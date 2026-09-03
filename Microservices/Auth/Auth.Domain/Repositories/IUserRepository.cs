using Auth.Domain.Entities;

namespace Auth.Domain.Repositories;

public interface IUserRepository
{
    Task<UserEntity?> GetByUsername(string username, CancellationToken ct = default);

    /// <summary>Usuario asociado a un cliente del negocio, si ya tiene uno.</summary>
    Task<UserEntity?> GetByClientId(long clientId, CancellationToken ct = default);

    Task<long> Insert(UserEntity entity, CancellationToken ct = default);

    /// <summary>Actualiza el contador de intentos fallidos y el bloqueo.</summary>
    Task<long> RegisterLoginAttempt(
        long userId, bool succeeded, int maxAttempts, int lockMinutes, CancellationToken ct = default);

    /// <summary>
    /// Baja lógica. El usuario deja de poder autenticarse pero se conserva el
    /// rastro de quién era para los registros históricos.
    /// </summary>
    Task<long> Delete(long userId, CancellationToken ct = default);
}
