using Core.Infrastructure.Database.Commands;
using Country.Domain.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Country.Infrastructure.Persistence.Commands;

/// <summary>SP dbo.INSERT_PAIS — devuelve el PAIS_ID_IT generado.</summary>
public class InsertCountryCommand(CountryEntity entity) : SqlCommandBase<long>
{
    public override string Name => "travelfake.INSERT_PAIS";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@PAIS_NOMBRE_VC", SqlDbType = SqlDbType.NVarChar, Size = 100, Value = entity.Name },
        new() { ParameterName = "@PAIS_CODIGO_VC", SqlDbType = SqlDbType.NVarChar, Size = 100, Value = entity.Code },
    ];
}
