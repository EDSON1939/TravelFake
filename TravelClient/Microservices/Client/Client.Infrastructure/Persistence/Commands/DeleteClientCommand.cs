using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Client.Infrastructure.Persistence.Commands;

/// <summary>
/// SP travelfake.DELETE_CLIENTE — baja física.
/// Devuelve 1 = eliminado, 0 = no encontrado.
/// </summary>
public class DeleteClientCommand(long customerId) : SqlCommandBase<long>
{
    public override string Name => "travelfake.DELETE_CLIENTE";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@CLIENTES_ID_IT", SqlDbType = SqlDbType.BigInt, Value = customerId },
    ];
}
