using Qr.Domain.Entities;

namespace Qr.Application.Common;

public record QrResponse(
    long QrId,
    string Codigo,
    long ComercioId,
    decimal Monto,
    string Tipo,
    DateTime FechaExpiracion,
    string Estado,
    bool Activo,
    DateTime? FechaActualizacion,
    DateTime FechaCreacion)
{
    // El QR es válido si su estado es ACTIVE
    public bool EsValido => Estado == QrEstado.ACTIVE;
}