using Core.Infrastructure.Database.Queries;
using Country.Domain.Entities;
using Dapper;
using System.Data;

namespace Country.Infrastructure.Persistence.Queries;

/// <summary>SP dbo.GET_PAIS_BY_ID.</summary>
public class GetCountryByIdQuery(long countryId) : QuerySingleBase<CountryEntity>
{
    public override string? Procedure => "travelfake.GET_PAIS_BY_ID";

    public override DynamicParameters? Parameters
    {
        get
        {
            var p = new DynamicParameters();
            p.Add("@PAIS_ID_IT", countryId, DbType.Int64);
            return p;
        }
    }
}
