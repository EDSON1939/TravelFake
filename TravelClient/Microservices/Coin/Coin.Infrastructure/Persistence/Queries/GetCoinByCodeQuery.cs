using Coin.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;

namespace Coin.Infrastructure.Persistence.Queries;

public class GetCoinByCodeQuery(string code) : QuerySingleBase<CoinEntity>
{
    public override string SqlStatement => @"
        SELECT MONE_ID_IT               AS CoinId,
               MONE_NOMBRE_VC           AS Name,
               MONE_CODIGO_VC           AS Code,
               MONE_SIMBOLO_VC          AS Symbol,
               MONE_ACTIVO_BT           AS IsActive,
               MONE_FECHA_CREACION_DT   AS CreatedAt
        FROM   coin.MONEDA
        WHERE  MONE_CODIGO_VC = @Code";

    public override DynamicParameters? Parameters
    {
        get
        {
            var p = new DynamicParameters();
            p.Add("@Code", code);
            return p;
        }
    }
}
