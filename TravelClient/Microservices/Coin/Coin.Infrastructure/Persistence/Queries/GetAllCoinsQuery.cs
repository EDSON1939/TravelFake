using Coin.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;

namespace Coin.Infrastructure.Persistence.Queries;

public class GetAllCoinsQuery(bool onlyActive) : QueryMultipleBase<CoinEntity>
{
    public override string SqlStatement => @"
        SELECT MONE_ID_IT               AS CoinId,
               MONE_NOMBRE_VC           AS Name,
               MONE_CODIGO_VC           AS Code,
               MONE_SIMBOLO_VC          AS Symbol,
               MONE_ACTIVO_BT           AS IsActive,
               MONE_FECHA_CREACION_DT   AS CreatedAt
        FROM   coin.MONEDA
        WHERE  (@OnlyActive = 0 OR MONE_ACTIVO_BT = 1)
        ORDER BY MONE_NOMBRE_VC ASC";

    public override DynamicParameters? Parameters
    {
        get
        {
            var p = new DynamicParameters();
            p.Add("@OnlyActive", onlyActive ? 1 : 0);
            return p;
        }
    }
}
