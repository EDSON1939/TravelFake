using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Client.Infrastructure.Persistence.Commands;

/// <summary>SP travelfake.UPDATE_CLIENTE_STATUS — baja/alta lógica. Devuelve las filas afectadas.</summary>
public class UpdateClientStatusCommand(long customerId, bool isActive) : SqlCommandBase<long>
{
    public override string Name => "travelfake.UPDATE_CLIENTE_STATUS";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@CLIENTES_ID_IT",     SqlDbType = SqlDbType.BigInt, Value = customerId },
        new() { ParameterName = "@CLIENTES_ACTIVO_BT", SqlDbType = SqlDbType.Bit,    Value = isActive   },
    ];
}
