using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Coin.Infrastructure.Persistence.Commands;

public class UpdateCoinStatusCommand(long coinId, bool isActive) : SqlCommandBase<long>
{
    public override string Name => "coin.UPDATE_MONEDA_STATUS";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@MONE_ID_IT",     SqlDbType = SqlDbType.BigInt, Value = coinId   },
        new() { ParameterName = "@MONE_ACTIVO_BT", SqlDbType = SqlDbType.Bit,    Value = isActive },
    ];
}
