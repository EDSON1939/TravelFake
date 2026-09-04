using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Core.Infrastructure.Audit
{
    /// <summary>
    /// Escribe una fila en aud.BITACORA. El procedimiento se encarga ademas de
    /// adoptar las filas de aud.AUDITORIA que dejo esta misma llamada,
    /// enlazandolas por TraceId.
    /// </summary>
    public class InsertBitacoraCommand(BitacoraEntry entry) : SqlCommandBase
    {
        public override string Name => "aud.INSERT_BITACORA";

        // Los nulos van como DBNull explicito: la sobrecarga no generica de
        // CommandConnection no los convierte, y un SqlParameter con Value null
        // hace que SqlClient omita el parametro.
        public override IEnumerable<SqlParameter>? Parameters =>
        [
            Text("@SERVICIO_VC",   entry.Service,     60),
            Text("@METODO_VC",     entry.Method,      160),
            Text("@OPERACION_VC",  entry.Operation,   10),
            Number("@USUARIO_ID_IT", entry.UserId),
            Text("@USUARIO_VC",    entry.Username,    60),
            Number("@CLIENTE_ID_IT", entry.ClientId),
            Text("@ROL_VC",        entry.Role,        20),
            Text("@IP_VC",         entry.Ip,          45),
            Text("@TRAZA_VC",      entry.TraceId,     64),
            Unicode("@PETICION_NV", entry.Request,    -1),
            Text("@ESTADO_VC",     entry.State,       10),
            Text("@CODIGO_VC",     entry.Code,        40),
            Unicode("@MENSAJE_NV", entry.Message,     500),
            new SqlParameter
            {
                ParameterName = "@DURACION_IT",
                SqlDbType     = SqlDbType.Int,
                Value         = entry.ElapsedMilliseconds
            }
        ];

        private static SqlParameter Text(string name, string? value, int size)
            => new()
            {
                ParameterName = name,
                SqlDbType     = SqlDbType.VarChar,
                Size          = size,
                Value         = (object?)value ?? DBNull.Value
            };

        private static SqlParameter Unicode(string name, string? value, int size)
            => new()
            {
                ParameterName = name,
                SqlDbType     = SqlDbType.NVarChar,
                Size          = size,
                Value         = (object?)value ?? DBNull.Value
            };

        private static SqlParameter Number(string name, long? value)
            => new()
            {
                ParameterName = name,
                SqlDbType     = SqlDbType.BigInt,
                Value         = (object?)value ?? DBNull.Value
            };
    }

    /// <summary>Una llamada atendida, tal como se guarda en la bitacora.</summary>
    public record BitacoraEntry
    {
        public required string  Service   { get; init; }
        public required string  Method    { get; init; }
        public required string  Operation { get; init; }
        public required string  State     { get; init; }

        public long?   UserId   { get; init; }
        public string? Username { get; init; }
        public long?   ClientId { get; init; }
        public string? Role     { get; init; }
        public string? Ip       { get; init; }
        public string? TraceId  { get; init; }
        public string? Request  { get; init; }
        public string? Code     { get; init; }
        public string? Message  { get; init; }

        public int ElapsedMilliseconds { get; init; }
    }
}
