using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.ExternalServices;
using System.Collections.Concurrent;

namespace AccountsAndMovements.Infrastructure.ExternalServices.Fakes;

/// <summary>
/// QRs quemados. A diferencia de los otros fakes este guarda estado -Consume
/// cambia el QR a USED-, asi que se registra como Singleton: con Scoped cada
/// request veria de nuevo el QR activo y no se podria probar el flujo completo.
///
/// Recordar igual que el uso unico del QR NO depende de este estado: lo garantiza
/// el indice UQ_COMMERCE_MOVIMIENTO_QR del propio servicio.
/// </summary>
public class FakeQrService : IQrService
{
    private readonly ConcurrentDictionary<string, QrInfo> _qrs;

    public FakeQrService()
    {
        var expiresAt = DateTime.Now.AddDays(1);

        _qrs = new ConcurrentDictionary<string, QrInfo>(StringComparer.OrdinalIgnoreCase)
        {
            // Escenario feliz del reto: 20 USD x 6.96 = 139.20 BOB exactos.
            ["QR-BO-0001"] = new(1, "QR-BO-0001", 55, 139.20m, QrType.UNICO, QrStatus.ACTIVE, true, expiresAt),
            // Monto 0 = QR abierto: acepta cualquier importe.
            ["QR-BO-0002"] = new(2, "QR-BO-0002", 56,      0m, QrType.UNICO, QrStatus.ACTIVE, true, expiresAt),
            // Vencido por fecha aunque el estado siga ACTIVE: manda la fecha.
            ["QR-BO-0003"] = new(3, "QR-BO-0003", 55,   50.00m, QrType.UNICO, QrStatus.ACTIVE, true, DateTime.Now.AddHours(-1)),
            ["QR-BO-0004"] = new(4, "QR-BO-0004", 55,   70.00m, QrType.UNICO, QrStatus.USED,   true, expiresAt),
            // Dado de baja: el estado sigue ACTIVE y lo que manda es "activo".
            ["QR-BO-0005"] = new(5, "QR-BO-0005", 55,   90.00m, QrType.UNICO, QrStatus.ACTIVE, false, expiresAt),
            // Apunta a un comercio dado de baja: COMMERCE_INACTIVE.
            ["QR-BO-0006"] = new(6, "QR-BO-0006", 99,   30.00m, QrType.UNICO, QrStatus.ACTIVE, true, expiresAt),
            // Cobro recurrente: el libro mayor no lo soporta, QR_TYPE_NOT_SUPPORTED.
            ["QR-BO-0007"] = new(7, "QR-BO-0007", 55,   45.00m, QrType.MULTIPLE, QrStatus.ACTIVE, true, expiresAt),
        };
    }

    public Task<QrInfo?> GetByCode(string code, CancellationToken ct = default)
        => Task.FromResult<QrInfo?>(_qrs.TryGetValue(code, out var qr) ? qr : null);

    public Task<bool> Consume(string code, CancellationToken ct = default)
    {
        if (!_qrs.TryGetValue(code, out var qr))
            return Task.FromResult(false);

        _qrs[code] = qr with { Status = QrStatus.USED };
        return Task.FromResult(true);
    }
}
