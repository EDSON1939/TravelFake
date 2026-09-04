using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Country.Infrastructure.Persistence.Commands;

/// <summary>SP dbo.UPDATE_PAIS_STATUS — baja/alta lógica. Devuelve las filas afectadas.</summary>
public class UpdateCountryStatusCommand(long countryId, bool isActive) : SqlCommandBase<long>
{
    public override string Name => "travelfake.UPDATE_PAIS_STATUS";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@PAIS_ID_IT",     SqlDbType = SqlDbType.BigInt, Value = countryId },
        new() { ParameterName = "@PAIS_ACTIVO_BT", SqlDbType = SqlDbType.Bit,    Value = isActive  },
    ];
}
