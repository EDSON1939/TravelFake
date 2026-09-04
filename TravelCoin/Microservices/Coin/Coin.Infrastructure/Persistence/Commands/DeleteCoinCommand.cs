using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Coin.Infrastructure.Persistence.Commands;

/// <summary>
/// SP travelfake.DELETE_MONEDA — hard delete.
/// Returns 1 = deleted, 0 = not found.
/// </summary>
public class DeleteCoinCommand(long coinId) : SqlCommandBase<long>
{
    public override string Name => "travelfake.DELETE_MONEDA";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@MONEDAS_ID_IT", SqlDbType = SqlDbType.BigInt, Value = coinId },
    ];
}
