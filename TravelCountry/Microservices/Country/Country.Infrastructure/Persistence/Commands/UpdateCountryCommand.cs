using Core.Infrastructure.Database.Commands;
using Country.Domain.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Country.Infrastructure.Persistence.Commands;

/// <summary>SP dbo.UPDATE_PAIS — devuelve las filas afectadas.</summary>
public class UpdateCountryCommand(CountryEntity entity) : SqlCommandBase<long>
{
    public override string Name => "travelfake.UPDATE_PAIS";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@PAIS_ID_IT",     SqlDbType = SqlDbType.BigInt,                Value = entity.CountryId },
        new() { ParameterName = "@PAIS_NOMBRE_VC", SqlDbType = SqlDbType.NVarChar, Size = 100,  Value = entity.Name      },
        new() { ParameterName = "@PAIS_CODIGO_VC", SqlDbType = SqlDbType.NVarChar, Size = 100,  Value = entity.Code      },
        new() { ParameterName = "@PAIS_ACTIVO_BT", SqlDbType = SqlDbType.Bit,                   Value = entity.IsActive  },
    ];
}
