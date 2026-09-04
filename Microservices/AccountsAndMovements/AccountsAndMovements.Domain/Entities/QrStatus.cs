namespace AccountsAndMovements.Domain.Entities;

/// <summary>
/// Estados del QR segun los publica el microservicio de QR (campo "estado").
///
/// El contrato oficial no tiene CANCELLED: un QR dado de baja llega con
/// estado ACTIVE y el flag "activo" en false, que es lo que mira
/// <see cref="ExternalServices.QrInfo.IsActive"/>.
/// </summary>
public struct QrStatus
{
    public const string ACTIVE  = nameof(ACTIVE);
    public const string EXPIRED = nameof(EXPIRED);
    public const string USED    = nameof(USED);
}
