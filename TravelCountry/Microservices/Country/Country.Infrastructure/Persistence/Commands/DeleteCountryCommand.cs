using Core.Infrastructure.Database.Commands;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Country.Infrastructure.Persistence.Commands;

/// <summary>
/// SP dbo.DELETE_PAIS — baja física.
/// Devuelve 1 = eliminado, 0 = no encontrado, -1 = tiene clientes asociados (FK_PAIS).
/// </summary>
public class DeleteCountryCommand(long countryId) : SqlCommandBase<long>
{
    public override string Name => "travelfake.DELETE_PAIS";

    public override IEnumerable<SqlParameter>? Parameters =>
    [
        new() { ParameterName = "@PAIS_ID_IT", SqlDbType = SqlDbType.BigInt, Value = countryId },
    ];
}
