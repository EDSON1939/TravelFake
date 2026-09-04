using Core.Infrastructure.Database.Queries;
using Country.Domain.Entities;
using Dapper;
using System.Data;

namespace Country.Infrastructure.Persistence.Queries;

/// <summary>
/// SP dbo.GET_PAIS_ALL. Al sobreescribir <see cref="QueryBase.Procedure"/>,
/// QueryBase resuelve ExecutionType = CommandType.StoredProcedure.
/// </summary>
public class GetAllCountriesQuery(bool onlyActive) : QueryMultipleBase<CountryEntity>
{
    public override string? Procedure => "travelfake.GET_PAIS_ALL";

    public override DynamicParameters? Parameters
    {
        get
        {
            var p = new DynamicParameters();
            p.Add("@PAIS_SOLO_ACTIVOS_BT", onlyActive, DbType.Boolean);
            return p;
        }
    }
}
