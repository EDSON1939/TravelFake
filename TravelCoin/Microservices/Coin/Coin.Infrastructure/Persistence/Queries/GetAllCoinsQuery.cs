using Coin.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;
using System.Data;

namespace Coin.Infrastructure.Persistence.Queries;

/// <summary>
/// SP travelfake.GET_MONEDA_ALL. Overriding <see cref="QueryBase.Procedure"/> makes
/// QueryBase resolve ExecutionType = CommandType.StoredProcedure.
/// </summary>
public class GetAllCoinsQuery(bool onlyActive) : QueryMultipleBase<CoinEntity>
{
    public override string? Procedure => "travelfake.GET_MONEDA_ALL";

    public override DynamicParameters? Parameters
    {
        get
        {
            var p = new DynamicParameters();
            p.Add("@MONEDAS_SOLO_ACTIVAS_BT", onlyActive, DbType.Boolean);
            return p;
        }
    }
}
