using Coin.Domain.Entities;
using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Coin.Infrastructure.Persistence.Commands;

/// <summary>SP travelfake.UPDATE_MONEDA — returns the number of affected rows.</summary>
public class UpdateCoinCommand(CoinEntity entity) : SqlCommandBase<long>
{
    public override string Name => "travelfake.UPDATE_MONEDA";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@MONEDAS_ID_IT",       SqlDbType = SqlDbType.BigInt,                             Value = entity.CoinId   },
        new() { ParameterName = "@MONEDAS_NOMBRE_VC",   SqlDbType = SqlDbType.NVarChar, Size = 100,               Value = entity.Name     },
        new() { ParameterName = "@MONEDAS_CODIGO_VC",   SqlDbType = SqlDbType.NVarChar, Size = 100,               Value = entity.Code     },
        new() { ParameterName = "@MONEDAS_TCCOMPRA_DC", SqlDbType = SqlDbType.Decimal,  Precision = 18, Scale = 2, Value = entity.BuyRate  },
        new() { ParameterName = "@MONEDAS_ACTIVO_BT",   SqlDbType = SqlDbType.Bit,                                Value = entity.IsActive },
    ];
}
