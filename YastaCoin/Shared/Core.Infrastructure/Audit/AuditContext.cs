using Core.ShareKernel.Security;
using System.Diagnostics;

namespace Core.Infrastructure.Audit
{
    /// <summary>
    /// Resuelve la traza y el usuario de la llamada en curso.
    ///
    /// La traza sale de <see cref="Activity.Current"/>, que es lo que ya usa
    /// OpenTelemetry y lo que Serilog imprime como TraceId en cada linea de log.
    /// Reutilizarla en vez de inventar un identificador propio es lo que permite
    /// saltar de una fila de auditoria al log y a la traza distribuida de la
    /// misma operacion.
    ///
    /// Fuera de un request no hay actividad y todo queda en null: una carga por
    /// script deja auditoria sin traza, que es correcto.
    /// </summary>
    public class AuditContext(ICurrentUser currentUser) : IAuditContext
    {
        public string? TraceId => Activity.Current?.TraceId.ToString();

        public long? UserId => currentUser.UserId;
    }
}
