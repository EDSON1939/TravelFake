using Coin.Domain.Entities;
using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Coin.Infrastructure.Persistence.Commands;

/// <summary>SP travelfake.INSERT_MONEDA — returns the generated MONEDAS_ID_IT.</summary>
public class InsertCoinCommand(CoinEntity entity) : SqlCommandBase<long>
{
    public override string Name => "travelfake.INSERT_MONEDA";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@MONEDAS_NOMBRE_VC",   SqlDbType = SqlDbType.NVarChar, Size = 100,               Value = entity.Name    },
        new() { ParameterName = "@MONEDAS_CODIGO_VC",   SqlDbType = SqlDbType.NVarChar, Size = 100,               Value = entity.Code    },
        new() { ParameterName = "@MONEDAS_TCCOMPRA_DC", SqlDbType = SqlDbType.Decimal,  Precision = 18, Scale = 2, Value = entity.BuyRate },
    ];
}
