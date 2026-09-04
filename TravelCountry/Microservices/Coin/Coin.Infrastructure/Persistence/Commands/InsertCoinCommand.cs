using Coin.Domain.Entities;
using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Coin.Infrastructure.Persistence.Commands;

public class InsertCoinCommand(CoinEntity entity) : SqlCommandBase<long>
{
    public override string Name => "coin.INSERT_MONEDA";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@MONE_NOMBRE_VC",  SqlDbType = SqlDbType.NVarChar, Size = 100, Value = entity.Name   },
        new() { ParameterName = "@MONE_CODIGO_VC",  SqlDbType = SqlDbType.VarChar,  Size = 10,  Value = entity.Code   },
        new() { ParameterName = "@MONE_SIMBOLO_VC", SqlDbType = SqlDbType.VarChar,  Size = 10,  Value = entity.Symbol },
    ];
}
