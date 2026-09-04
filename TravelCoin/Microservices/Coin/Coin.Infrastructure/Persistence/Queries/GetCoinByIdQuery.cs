using Coin.Domain.Entities;
using Core.Infrastructure.Database.Queries;
using Dapper;
using System.Data;

namespace Coin.Infrastructure.Persistence.Queries;

/// <summary>SP travelfake.GET_MONEDA_BY_ID.</summary>
public class GetCoinByIdQuery(long coinId) : QuerySingleBase<CoinEntity>
{
    public override string? Procedure => "travelfake.GET_MONEDA_BY_ID";

    public override DynamicParameters? Parameters
    {
        get
        {
            var p = new DynamicParameters();
            p.Add("@MONEDAS_ID_IT", coinId, DbType.Int64);
            return p;
        }
    }
}
