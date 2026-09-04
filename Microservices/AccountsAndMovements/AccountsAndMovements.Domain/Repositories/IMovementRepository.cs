using AccountsAndMovements.Domain.Entities;

namespace AccountsAndMovements.Domain.Repositories;

public interface IMovementRepository
{
    Task<IEnumerable<MovementEntity>> GetByAccount(
        string accountNumber, int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>Historial del reto: titular, rango de fechas y estado, paginado.</summary>
    Task<IEnumerable<MovementEntity>> GetHistory(
        string accountType, long holderId, DateTime? from, DateTime? to,
        string? status, int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Devuelve el DEBITO ya aplicado con esa clave de idempotencia, si existe.
    /// Es lo que permite responder el resultado original en vez de cobrar dos veces.
    /// </summary>
    Task<MovementEntity?> GetByIdempotencyKey(string idempotencyKey, CancellationToken ct = default);

    /// <summary>Los dos asientos de una operacion: el debito del cliente y el credito del comercio.</summary>
    Task<IEnumerable<MovementEntity>> GetByTransactionCode(
        string transactionCode, CancellationToken ct = default);

    /// <summary>
    /// Credito o debito suelto sobre una cuenta (apertura, recarga, reversa).
    /// Idempotente por (cuenta, clave). Devuelve el ID del asiento o un codigo
    /// negativo de <see cref="PaymentResult"/>.
    /// </summary>
    Task<long> ApplyMovement(MovementEntity movement, CancellationToken ct = default);

    /// <summary>
    /// Ejecuta el pago QR completo -debito al cliente, credito al comercio y los
    /// dos asientos- en una sola transaccion. Devuelve el ID del asiento de
    /// debito o un codigo negativo de <see cref="PaymentResult"/>.
    /// </summary>
    Task<long> ExecuteQrPayment(PaymentEntity payment, CancellationToken ct = default);
}
