using Microsoft.Data.SqlClient;
using System.Data;

namespace Core.Infrastructure.Audit
{
    /// <summary>
    /// Los dos parametros que todo stored procedure que cambia datos recibe para
    /// poder dejar auditoria. Estan en un solo lugar para que los procedimientos
    /// no terminen recibiendolos con nombres distintos.
    /// </summary>
    public static class AuditParameters
    {
        public static SqlParameter[] For(IAuditContext? context) =>
        [
            new()
            {
                ParameterName = "@AUDITORIA_TRAZA_VC",
                SqlDbType     = SqlDbType.VarChar,
                Size          = 64,
                Value         = (object?)context?.TraceId ?? DBNull.Value
            },
            new()
            {
                ParameterName = "@AUDITORIA_USUARIO_IT",
                SqlDbType     = SqlDbType.BigInt,
                Value         = (object?)context?.UserId ?? DBNull.Value
            }
        ];
    }
}
