using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Coin.Infrastructure.Persistence.Commands;

/// <summary>SP travelfake.UPDATE_MONEDA_STATUS — soft delete/restore. Returns affected rows.</summary>
public class UpdateCoinStatusCommand(long coinId, bool isActive) : SqlCommandBase<long>
{
    public override string Name => "travelfake.UPDATE_MONEDA_STATUS";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@MONEDAS_ID_IT",     SqlDbType = SqlDbType.BigInt, Value = coinId   },
        new() { ParameterName = "@MONEDAS_ACTIVO_BT", SqlDbType = SqlDbType.Bit,    Value = isActive },
    ];
}
