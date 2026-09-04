using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.ExternalServices;
using System.Collections.Concurrent;

namespace AccountsAndMovements.Infrastructure.ExternalServices.Fakes;

/// <summary>
/// QRs quemados. A diferencia de los otros fakes este guarda estado -MarkAsUsed
/// cambia el QR a USED-, asi que se registra como Singleton: con Scoped cada
/// request veria de nuevo el QR activo y no se podria probar el flujo completo.
///
/// Recordar igual que el uso unico del QR NO depende de este estado: lo garantiza
/// el indice UQ_PAY_MOVIMIENTO_QR del propio servicio.
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
            ["QR-BO-0001"] = new("QR-BO-0001", 55, 139.20m, "BOB", "REF-CAFE-001",  QrStatus.ACTIVE,    expiresAt),
            // Monto 0 = QR abierto: acepta cualquier importe.
            ["QR-BO-0002"] = new("QR-BO-0002", 56,      0m, "BOB", "REF-ARTE-002",  QrStatus.ACTIVE,    expiresAt),
            // Vencido por fecha aunque el estado siga ACTIVE: manda la fecha.
            ["QR-BO-0003"] = new("QR-BO-0003", 55,   50.00m, "BOB", "REF-VENC-003", QrStatus.ACTIVE,    DateTime.Now.AddHours(-1)),
            ["QR-BO-0004"] = new("QR-BO-0004", 55,   70.00m, "BOB", "REF-USADO-004", QrStatus.USED,     expiresAt),
            ["QR-BO-0005"] = new("QR-BO-0005", 55,   90.00m, "BOB", "REF-CANC-005", QrStatus.CANCELLED, expiresAt),
            // Apunta a un comercio dado de baja: MERCHANT_INACTIVE.
            ["QR-BO-0006"] = new("QR-BO-0006", 99,   30.00m, "BOB", "REF-BAJA-006", QrStatus.ACTIVE,    expiresAt),
        };
    }

    public Task<QrInfo?> GetByCode(string code, CancellationToken ct = default)
        => Task.FromResult<QrInfo?>(_qrs.TryGetValue(code, out var qr) ? qr : null);

    public Task<bool> MarkAsUsed(string code, string transactionCode, CancellationToken ct = default)
    {
        if (!_qrs.TryGetValue(code, out var qr))
            return Task.FromResult(false);

        _qrs[code] = qr with { Status = QrStatus.USED };
        return Task.FromResult(true);
    }
}
